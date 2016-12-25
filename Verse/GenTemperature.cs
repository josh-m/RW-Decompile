using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenTemperature
	{
		public static readonly Color ColorSpotHot = new Color(1f, 0f, 0f, 0.6f);

		public static readonly Color ColorSpotCold = new Color(0f, 0f, 1f, 0.6f);

		public static readonly Color ColorRoomHot = new Color(1f, 0f, 0f, 0.3f);

		public static readonly Color ColorRoomCold = new Color(0f, 0f, 1f, 0.3f);

		private static List<Room> neighRooms = new List<Room>();

		private static Room[] beqRooms = new Room[4];

		public static float AverageTemperatureAtTileForMonth(int tile, Month month)
		{
			int num = 30000;
			int num2 = 300000 * (int)month;
			float num3 = 0f;
			for (int i = 0; i < 120; i++)
			{
				int absTick = num2 + num + Mathf.RoundToInt((float)i / 120f * 300000f);
				num3 += GenTemperature.GetTemperatureFromSeasonAtTile(absTick, tile);
			}
			return num3 / 120f;
		}

		public static FloatRange ComfortableTemperatureRange(this Pawn p)
		{
			return new FloatRange(p.GetStatValue(StatDefOf.ComfyTemperatureMin, true), p.GetStatValue(StatDefOf.ComfyTemperatureMax, true));
		}

		public static FloatRange SafeTemperatureRange(this Pawn p)
		{
			FloatRange result = p.ComfortableTemperatureRange();
			result.min -= 10f;
			result.max += 10f;
			return result;
		}

		public static float GetTemperatureForCell(IntVec3 c, Map map)
		{
			float result;
			GenTemperature.TryGetTemperatureForCell(c, map, out result);
			return result;
		}

		public static bool TryGetTemperatureForCell(IntVec3 c, Map map, out float tempResult)
		{
			if (map == null)
			{
				Log.Error("Got temperature for null map.");
				tempResult = 21f;
				return true;
			}
			if (!c.InBounds(map))
			{
				tempResult = 21f;
				return false;
			}
			if (GenTemperature.TryGetDirectAirTemperatureForCell(c, map, out tempResult))
			{
				return true;
			}
			List<Thing> list = map.thingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability == Traversability.Impassable)
				{
					return GenTemperature.TryGetAirTemperatureAroundThing(list[i], out tempResult);
				}
			}
			return false;
		}

		public static bool TryGetDirectAirTemperatureForCell(IntVec3 c, Map map, out float temperature)
		{
			if (!c.InBounds(map))
			{
				temperature = 21f;
				return false;
			}
			if (c.Impassable(map))
			{
				temperature = 21f;
				return false;
			}
			Room room = RoomQuery.RoomAt(c, map);
			if (room == null)
			{
				temperature = 21f;
				return false;
			}
			temperature = room.Temperature;
			return true;
		}

		public static bool TryGetAirTemperatureAroundThing(Thing t, out float temperature)
		{
			float num = 0f;
			int num2 = 0;
			List<IntVec3> list = GenAdjFast.AdjacentCells8Way(t);
			for (int i = 0; i < list.Count; i++)
			{
				float num3;
				if (list[i].InBounds(t.Map) && GenTemperature.TryGetDirectAirTemperatureForCell(list[i], t.Map, out num3))
				{
					num += num3;
					num2++;
				}
			}
			if (num2 > 0)
			{
				temperature = num / (float)num2;
				return true;
			}
			temperature = 21f;
			return false;
		}

		public static float GetTemperatureAtCellOrCaravanTile(Pawn pawn)
		{
			float result;
			if (!GenTemperature.TryGetTemperatureAtCellOrCaravanTile(pawn, out result))
			{
				result = 21f;
			}
			return result;
		}

		public static bool TryGetTemperatureAtCellOrCaravanTile(Pawn pawn, out float temp)
		{
			if (pawn.MapHeld != null)
			{
				temp = GenTemperature.GetTemperatureForCell(pawn.PositionHeld, pawn.MapHeld);
				return true;
			}
			Caravan caravan = pawn.GetCaravan();
			if (caravan != null)
			{
				temp = GenTemperature.AverageTemperatureAtTileForMonth(caravan.Tile, GenLocalDate.Month(caravan.Tile));
				return true;
			}
			temp = 21f;
			return false;
		}

		public static float OffsetFromSunCycle(int absTick, int tile)
		{
			float num = GenDate.DayPercent((long)absTick, Find.WorldGrid.LongLatOf(tile).x);
			float f = 6.28318548f * (num + 0.32f);
			return Mathf.Cos(f) * 7f;
		}

		public static float OffsetFromSeasonCycle(int absTick, int tile)
		{
			float num = (float)(absTick / 60000 % 60) / 60f;
			float f = 6.28318548f * num;
			return Mathf.Cos(f) * -GenTemperature.SeasonalShiftAmplitudeAt(tile);
		}

		public static float GetTemperatureFromSeasonAtTile(int absTick, int tile)
		{
			if (absTick == 0)
			{
				absTick = 1;
			}
			Tile tile2 = Find.WorldGrid[tile];
			return tile2.temperature + GenTemperature.OffsetFromSeasonCycle(absTick, tile);
		}

		public static float SeasonalShiftAmplitudeAt(int tile)
		{
			return TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
		}

		public static List<Month> MonthsInTemperatureRange(int tile, float minTemp, float maxTemp)
		{
			List<Month> months = new List<Month>();
			for (int i = 0; i < 12; i++)
			{
				float num = GenTemperature.AverageTemperatureAtTileForMonth(tile, (Month)i);
				if (num >= minTemp && num <= maxTemp)
				{
					months.Add((Month)i);
				}
			}
			if (months.Count <= 1 || months.Count == 12)
			{
				return months;
			}
			if (months.Contains(Month.Dec) && months.Contains(Month.Jan))
			{
				Month month = months.First((Month m) => !months.Contains((Month)(m - Month.Feb)));
				List<Month> list = new List<Month>();
				for (int j = (int)month; j < 12; j++)
				{
					if (!months.Contains((Month)j))
					{
						break;
					}
					list.Add((Month)j);
				}
				for (int k = 0; k < 12; k++)
				{
					if (!months.Contains((Month)k))
					{
						break;
					}
					list.Add((Month)k);
				}
			}
			return months;
		}

		public static bool PushHeat(IntVec3 c, Map map, float energy)
		{
			Room room = c.GetRoom(map);
			if (room != null)
			{
				return room.PushHeat(energy);
			}
			GenTemperature.neighRooms.Clear();
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = c + GenAdj.AdjacentCells[i];
				if (intVec.InBounds(map))
				{
					room = intVec.GetRoom(map);
					if (room != null)
					{
						GenTemperature.neighRooms.Add(room);
					}
				}
			}
			float energy2 = energy / (float)GenTemperature.neighRooms.Count;
			for (int j = 0; j < GenTemperature.neighRooms.Count; j++)
			{
				GenTemperature.neighRooms[j].PushHeat(energy2);
			}
			return GenTemperature.neighRooms.Count > 0;
		}

		public static void PushHeat(Thing t, float energy)
		{
			IntVec3 c;
			if (t.def.passability != Traversability.Impassable)
			{
				GenTemperature.PushHeat(t.Position, t.Map, energy);
			}
			else if (GenAdj.TryFindRandomWalkableAdjacentCell8Way(t, out c))
			{
				GenTemperature.PushHeat(c, t.Map, energy);
			}
		}

		public static float ControlTemperatureTempChange(IntVec3 cell, Map map, float energyLimit, float targetTemperature)
		{
			Room room = RoomQuery.RoomAt(cell, map);
			if (room == null || room.UsesOutdoorTemperature)
			{
				return 0f;
			}
			float b = energyLimit / (float)room.CellCount;
			float a = targetTemperature - room.Temperature;
			float num;
			if (energyLimit > 0f)
			{
				num = Mathf.Min(a, b);
				num = Mathf.Max(num, 0f);
			}
			else
			{
				num = Mathf.Max(a, b);
				num = Mathf.Min(num, 0f);
			}
			return num;
		}

		public static void EqualizeTemperaturesThroughBuilding(Building b, float rate, bool twoWay)
		{
			int num = 0;
			float num2 = 0f;
			if (twoWay)
			{
				for (int i = 0; i < 2; i++)
				{
					IntVec3 intVec = (i != 0) ? (b.Position - b.Rotation.FacingCell) : (b.Position + b.Rotation.FacingCell);
					if (intVec.InBounds(b.Map))
					{
						Room room = intVec.GetRoom(b.Map);
						if (room != null)
						{
							num2 += room.Temperature;
							GenTemperature.beqRooms[num] = room;
							num++;
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < 4; j++)
				{
					IntVec3 intVec2 = b.Position + GenAdj.CardinalDirections[j];
					if (intVec2.InBounds(b.Map))
					{
						Room room2 = intVec2.GetRoom(b.Map);
						if (room2 != null)
						{
							num2 += room2.Temperature;
							GenTemperature.beqRooms[num] = room2;
							num++;
						}
					}
				}
			}
			if (num == 0)
			{
				return;
			}
			float num3 = num2 / (float)num;
			Room room3 = b.GetRoom();
			if (room3 != null)
			{
				room3.Temperature = num3;
			}
			if (num == 1)
			{
				return;
			}
			float num4 = 1f;
			for (int k = 0; k < num; k++)
			{
				if (!GenTemperature.beqRooms[k].UsesOutdoorTemperature)
				{
					float temperature = GenTemperature.beqRooms[k].Temperature;
					float num5 = num3 - temperature;
					float num6 = num5 * rate;
					float num7 = num6 / (float)GenTemperature.beqRooms[k].CellCount;
					float num8 = GenTemperature.beqRooms[k].Temperature + num7;
					if (num6 > 0f && num8 > num3)
					{
						num8 = num3;
					}
					else if (num6 < 0f && num8 < num3)
					{
						num8 = num3;
					}
					float num9 = Mathf.Abs((num8 - temperature) * (float)GenTemperature.beqRooms[k].CellCount / num6);
					if (num9 < num4)
					{
						num4 = num9;
					}
				}
			}
			for (int l = 0; l < num; l++)
			{
				if (!GenTemperature.beqRooms[l].UsesOutdoorTemperature)
				{
					float temperature2 = GenTemperature.beqRooms[l].Temperature;
					float num10 = num3 - temperature2;
					float num11 = num10 * rate * num4;
					float num12 = num11 / (float)GenTemperature.beqRooms[l].CellCount;
					GenTemperature.beqRooms[l].Temperature += num12;
				}
			}
		}

		public static float RotRateAtTemperature(float temperature)
		{
			if (temperature < 0f)
			{
				return 0f;
			}
			if (temperature >= 10f)
			{
				return 1f;
			}
			return temperature / 10f;
		}

		public static bool FactionOwnsRoomInTemperatureRange(Faction faction, FloatRange tempRange, Map map)
		{
			if (faction == Faction.OfPlayer)
			{
				List<Room> allRooms = map.regionGrid.allRooms;
				for (int i = 0; i < allRooms.Count; i++)
				{
					if (tempRange.Includes(allRooms[i].Temperature))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		public static float CelsiusTo(float temp, TemperatureDisplayMode oldMode)
		{
			switch (oldMode)
			{
			case TemperatureDisplayMode.Celsius:
				return temp;
			case TemperatureDisplayMode.Fahrenheit:
				return temp * 1.8f + 32f;
			case TemperatureDisplayMode.Kelvin:
				return temp + 273.15f;
			default:
				throw new InvalidOperationException();
			}
		}

		public static float CelsiusToOffset(float temp, TemperatureDisplayMode oldMode)
		{
			switch (oldMode)
			{
			case TemperatureDisplayMode.Celsius:
				return temp;
			case TemperatureDisplayMode.Fahrenheit:
				return temp * 1.8f;
			case TemperatureDisplayMode.Kelvin:
				return temp;
			default:
				throw new InvalidOperationException();
			}
		}

		public static float ConvertTemperatureOffset(float temp, TemperatureDisplayMode oldMode, TemperatureDisplayMode newMode)
		{
			switch (oldMode)
			{
			case TemperatureDisplayMode.Fahrenheit:
				temp /= 1.8f;
				break;
			}
			switch (newMode)
			{
			case TemperatureDisplayMode.Fahrenheit:
				temp *= 1.8f;
				break;
			}
			return temp;
		}
	}
}
