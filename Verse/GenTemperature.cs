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

		private static List<RoomGroup> neighRoomGroups = new List<RoomGroup>();

		private static RoomGroup[] beqRoomGroups = new RoomGroup[4];

		public static float AverageTemperatureAtTileForTwelfth(int tile, Twelfth twelfth)
		{
			int num = 30000;
			int num2 = 300000 * (int)twelfth;
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

		public static FloatRange ComfortableTemperatureRange(ThingDef raceDef, List<ThingStuffPair> apparel = null)
		{
			FloatRange result = new FloatRange(raceDef.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null), raceDef.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null));
			if (apparel != null)
			{
				result.min += apparel.Sum((ThingStuffPair x) => x.InsulationCold);
				result.max += apparel.Sum((ThingStuffPair x) => x.InsulationHeat);
			}
			return result;
		}

		public static FloatRange SafeTemperatureRange(this Pawn p)
		{
			FloatRange result = p.ComfortableTemperatureRange();
			result.min -= 10f;
			result.max += 10f;
			return result;
		}

		public static FloatRange SafeTemperatureRange(ThingDef raceDef, List<ThingStuffPair> apparel = null)
		{
			FloatRange result = GenTemperature.ComfortableTemperatureRange(raceDef, apparel);
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
			RoomGroup roomGroup = c.GetRoomGroup(map);
			if (roomGroup == null)
			{
				temperature = 21f;
				return false;
			}
			temperature = roomGroup.Temperature;
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

		public static float OffsetFromSunCycle(int absTick, int tile)
		{
			float num = GenDate.DayPercent((long)absTick, Find.WorldGrid.LongLatOf(tile).x);
			float f = 6.28318548f * (num + 0.32f);
			return Mathf.Cos(f) * 7f;
		}

		public static float OffsetFromSeasonCycle(int absTick, int tile)
		{
			float num = (float)(absTick / 60000 % 60) / 60f;
			float f = 6.28318548f * (num - Season.Winter.GetMiddleTwelfth(0f).GetBeginningYearPct());
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

		public static float GetTemperatureAtTile(int tile)
		{
			Map map = Current.Game.FindMap(tile);
			if (map != null)
			{
				return map.mapTemperature.OutdoorTemp;
			}
			return GenTemperature.GetTemperatureFromSeasonAtTile(GenTicks.TicksAbs, tile);
		}

		public static float SeasonalShiftAmplitudeAt(int tile)
		{
			if (Find.WorldGrid.LongLatOf(tile).y >= 0f)
			{
				return TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
			}
			return -TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
		}

		public static List<Twelfth> TwelfthsInAverageTemperatureRange(int tile, float minTemp, float maxTemp)
		{
			List<Twelfth> twelfths = new List<Twelfth>();
			for (int i = 0; i < 12; i++)
			{
				float num = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, (Twelfth)i);
				if (num >= minTemp && num <= maxTemp)
				{
					twelfths.Add((Twelfth)i);
				}
			}
			if (twelfths.Count <= 1 || twelfths.Count == 12)
			{
				return twelfths;
			}
			if (twelfths.Contains(Twelfth.Twelfth) && twelfths.Contains(Twelfth.First))
			{
				Twelfth twelfth = twelfths.First((Twelfth m) => !twelfths.Contains((Twelfth)(m - Twelfth.Second)));
				List<Twelfth> list = new List<Twelfth>();
				for (int j = (int)twelfth; j < 12; j++)
				{
					if (!twelfths.Contains((Twelfth)j))
					{
						break;
					}
					list.Add((Twelfth)j);
				}
				for (int k = 0; k < 12; k++)
				{
					if (!twelfths.Contains((Twelfth)k))
					{
						break;
					}
					list.Add((Twelfth)k);
				}
			}
			return twelfths;
		}

		public static Twelfth EarliestTwelfthInAverageTemperatureRange(int tile, float minTemp, float maxTemp)
		{
			int i = 0;
			while (i < 12)
			{
				float num = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, (Twelfth)i);
				if (num >= minTemp && num <= maxTemp)
				{
					if (i != 0)
					{
						return (Twelfth)i;
					}
					Twelfth twelfth = (Twelfth)i;
					for (int j = 0; j < 12; j++)
					{
						float num2 = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth.PreviousTwelfth());
						if (num2 < minTemp || num2 > maxTemp)
						{
							return twelfth;
						}
						twelfth = twelfth.PreviousTwelfth();
					}
					return (Twelfth)i;
				}
				else
				{
					i++;
				}
			}
			return Twelfth.Undefined;
		}

		public static bool PushHeat(IntVec3 c, Map map, float energy)
		{
			if (map == null)
			{
				Log.Error("Added heat to null map.");
				return false;
			}
			RoomGroup roomGroup = c.GetRoomGroup(map);
			if (roomGroup != null)
			{
				return roomGroup.PushHeat(energy);
			}
			GenTemperature.neighRoomGroups.Clear();
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = c + GenAdj.AdjacentCells[i];
				if (intVec.InBounds(map))
				{
					roomGroup = intVec.GetRoomGroup(map);
					if (roomGroup != null)
					{
						GenTemperature.neighRoomGroups.Add(roomGroup);
					}
				}
			}
			float energy2 = energy / (float)GenTemperature.neighRoomGroups.Count;
			for (int j = 0; j < GenTemperature.neighRoomGroups.Count; j++)
			{
				GenTemperature.neighRoomGroups[j].PushHeat(energy2);
			}
			bool result = GenTemperature.neighRoomGroups.Count > 0;
			GenTemperature.neighRoomGroups.Clear();
			return result;
		}

		public static void PushHeat(Thing t, float energy)
		{
			IntVec3 c;
			if (t.GetRoomGroup() != null)
			{
				GenTemperature.PushHeat(t.Position, t.Map, energy);
			}
			else if (GenAdj.TryFindRandomAdjacentCell8WayWithRoomGroup(t, out c))
			{
				GenTemperature.PushHeat(c, t.Map, energy);
			}
		}

		public static float ControlTemperatureTempChange(IntVec3 cell, Map map, float energyLimit, float targetTemperature)
		{
			RoomGroup roomGroup = cell.GetRoomGroup(map);
			if (roomGroup == null || roomGroup.UsesOutdoorTemperature)
			{
				return 0f;
			}
			float b = energyLimit / (float)roomGroup.CellCount;
			float a = targetTemperature - roomGroup.Temperature;
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
						RoomGroup roomGroup = intVec.GetRoomGroup(b.Map);
						if (roomGroup != null)
						{
							num2 += roomGroup.Temperature;
							GenTemperature.beqRoomGroups[num] = roomGroup;
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
						RoomGroup roomGroup2 = intVec2.GetRoomGroup(b.Map);
						if (roomGroup2 != null)
						{
							num2 += roomGroup2.Temperature;
							GenTemperature.beqRoomGroups[num] = roomGroup2;
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
			RoomGroup roomGroup3 = b.GetRoomGroup();
			if (roomGroup3 != null)
			{
				roomGroup3.Temperature = num3;
			}
			if (num == 1)
			{
				return;
			}
			float num4 = 1f;
			for (int k = 0; k < num; k++)
			{
				if (!GenTemperature.beqRoomGroups[k].UsesOutdoorTemperature)
				{
					float temperature = GenTemperature.beqRoomGroups[k].Temperature;
					float num5 = num3 - temperature;
					float num6 = num5 * rate;
					float num7 = num6 / (float)GenTemperature.beqRoomGroups[k].CellCount;
					float num8 = GenTemperature.beqRoomGroups[k].Temperature + num7;
					if (num6 > 0f && num8 > num3)
					{
						num8 = num3;
					}
					else if (num6 < 0f && num8 < num3)
					{
						num8 = num3;
					}
					float num9 = Mathf.Abs((num8 - temperature) * (float)GenTemperature.beqRoomGroups[k].CellCount / num6);
					if (num9 < num4)
					{
						num4 = num9;
					}
				}
			}
			for (int l = 0; l < num; l++)
			{
				if (!GenTemperature.beqRoomGroups[l].UsesOutdoorTemperature)
				{
					float temperature2 = GenTemperature.beqRoomGroups[l].Temperature;
					float num10 = num3 - temperature2;
					float num11 = num10 * rate * num4;
					float num12 = num11 / (float)GenTemperature.beqRoomGroups[l].CellCount;
					GenTemperature.beqRoomGroups[l].Temperature += num12;
				}
			}
			for (int m = 0; m < GenTemperature.beqRoomGroups.Length; m++)
			{
				GenTemperature.beqRoomGroups[m] = null;
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

		public static bool FactionOwnsPassableRoomInTemperatureRange(Faction faction, FloatRange tempRange, Map map)
		{
			if (faction == Faction.OfPlayer)
			{
				List<Room> allRooms = map.regionGrid.allRooms;
				for (int i = 0; i < allRooms.Count; i++)
				{
					Room room = allRooms[i];
					if (room.RegionType.Passable() && !room.Fogged && tempRange.Includes(room.Temperature))
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
			if (oldMode == TemperatureDisplayMode.Celsius)
			{
				return temp;
			}
			if (oldMode == TemperatureDisplayMode.Fahrenheit)
			{
				return temp * 1.8f + 32f;
			}
			if (oldMode != TemperatureDisplayMode.Kelvin)
			{
				throw new InvalidOperationException();
			}
			return temp + 273.15f;
		}

		public static float CelsiusToOffset(float temp, TemperatureDisplayMode oldMode)
		{
			if (oldMode == TemperatureDisplayMode.Celsius)
			{
				return temp;
			}
			if (oldMode == TemperatureDisplayMode.Fahrenheit)
			{
				return temp * 1.8f;
			}
			if (oldMode != TemperatureDisplayMode.Kelvin)
			{
				throw new InvalidOperationException();
			}
			return temp;
		}

		public static float ConvertTemperatureOffset(float temp, TemperatureDisplayMode oldMode, TemperatureDisplayMode newMode)
		{
			if (oldMode != TemperatureDisplayMode.Celsius)
			{
				if (oldMode != TemperatureDisplayMode.Fahrenheit)
				{
					if (oldMode != TemperatureDisplayMode.Kelvin)
					{
					}
				}
				else
				{
					temp /= 1.8f;
				}
			}
			if (newMode != TemperatureDisplayMode.Celsius)
			{
				if (newMode != TemperatureDisplayMode.Fahrenheit)
				{
					if (newMode != TemperatureDisplayMode.Kelvin)
					{
					}
				}
				else
				{
					temp *= 1.8f;
				}
			}
			return temp;
		}
	}
}
