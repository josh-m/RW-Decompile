using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.Noise;

namespace Verse
{
	public static class GenTemperature
	{
		private const int CachedTempUpdateInterval = 60;

		private static float cachedOutdoorTemp;

		private static float cachedSeasonalTemp;

		private static float[] cachedMonthlyTempAverages;

		private static Perlin dailyVariationPerlinCached;

		private static HashSet<int> fastProcessedRoomIDs;

		public static readonly Color ColorSpotHot;

		public static readonly Color ColorSpotCold;

		public static readonly Color ColorRoomHot;

		public static readonly Color ColorRoomCold;

		private static List<Room> neighRooms;

		public static float OutdoorTemp
		{
			get
			{
				if (GenTemperature.cachedOutdoorTemp == -3.40282347E+38f)
				{
					GenTemperature.UpdateCachedData();
				}
				return GenTemperature.cachedOutdoorTemp;
			}
		}

		public static float SeasonalTemp
		{
			get
			{
				if (GenTemperature.cachedSeasonalTemp == -3.40282347E+38f)
				{
					GenTemperature.UpdateCachedData();
				}
				return GenTemperature.cachedSeasonalTemp;
			}
		}

		private static Perlin DailyVariationPerlin
		{
			get
			{
				if (GenTemperature.dailyVariationPerlinCached == null)
				{
					int hashCode = Find.Map.WorldCoords.GetHashCode();
					GenTemperature.dailyVariationPerlinCached = new Perlin(4.9999998736893758E-06, 2.0, 0.5, 3, hashCode, QualityMode.Medium);
				}
				return GenTemperature.dailyVariationPerlinCached;
			}
		}

		static GenTemperature()
		{
			GenTemperature.cachedMonthlyTempAverages = null;
			GenTemperature.fastProcessedRoomIDs = new HashSet<int>();
			GenTemperature.ColorSpotHot = new Color(1f, 0f, 0f, 0.6f);
			GenTemperature.ColorSpotCold = new Color(0f, 0f, 1f, 0.6f);
			GenTemperature.ColorRoomHot = new Color(1f, 0f, 0f, 0.3f);
			GenTemperature.ColorRoomCold = new Color(0f, 0f, 1f, 0.3f);
			GenTemperature.neighRooms = new List<Room>();
			GenTemperature.Reinit();
		}

		public static void Reinit()
		{
			GenTemperature.dailyVariationPerlinCached = null;
			GenTemperature.cachedOutdoorTemp = -3.40282347E+38f;
			GenTemperature.cachedSeasonalTemp = -3.40282347E+38f;
			GenTemperature.cachedMonthlyTempAverages = null;
		}

		public static void GenTemperatureTick()
		{
			if (Find.TickManager.TicksGame % 60 == 4)
			{
				GenTemperature.UpdateCachedData();
			}
			if (Find.TickManager.TicksGame % 120 == 7)
			{
				GenTemperature.fastProcessedRoomIDs.Clear();
				foreach (Region current in Find.RegionGrid.AllRegions)
				{
					if (current.Room != null && !GenTemperature.fastProcessedRoomIDs.Contains(current.Room.ID))
					{
						current.Room.TempTracker.EqualizeTemperature();
						GenTemperature.fastProcessedRoomIDs.Add(current.Room.ID);
					}
				}
			}
		}

		public static void UpdateCachedData()
		{
			GenTemperature.cachedOutdoorTemp = GenTemperature.OutdoorTemperatureAt(Find.TickManager.TicksAbs);
			GenTemperature.cachedOutdoorTemp += Find.MapConditionManager.AggregateTemperatureOffset();
			GenTemperature.cachedSeasonalTemp = GenTemperature.CalculateOutdoorTemperatureAtWorldCoords(Find.TickManager.TicksAbs, Find.Map.WorldCoords, false);
		}

		public static float AverageTemperatureAtWorldCoordsForMonth(IntVec2 worldCoords, Month month)
		{
			int num = 30000;
			int num2 = 300000 * (int)month;
			float num3 = 0f;
			for (int i = 0; i < 120; i++)
			{
				int absTick = num2 + num + Mathf.RoundToInt((float)i / 120f * 300000f);
				num3 += GenTemperature.GetTemperatureFromSeasonAtWorldCoords(absTick, worldCoords);
			}
			return num3 / 120f;
		}

		public static bool SeasonAcceptableFor(ThingDef animalRace)
		{
			return GenTemperature.SeasonalTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) && GenTemperature.SeasonalTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
		}

		public static bool OutdoorTemperatureAcceptableFor(ThingDef animalRace)
		{
			return GenTemperature.OutdoorTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) && GenTemperature.OutdoorTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
		}

		public static bool SeasonAndOutdoorTemperatureAcceptableFor(ThingDef animalRace)
		{
			return GenTemperature.SeasonAcceptableFor(animalRace) && GenTemperature.OutdoorTemperatureAcceptableFor(animalRace);
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

		public static float GetTemperatureForCell(IntVec3 c)
		{
			float result;
			GenTemperature.TryGetTemperatureForCell(c, out result);
			return result;
		}

		public static bool TryGetTemperatureForCell(IntVec3 c, out float tempResult)
		{
			if (!c.InBounds())
			{
				tempResult = 21f;
				return false;
			}
			if (GenTemperature.TryGetDirectAirTemperatureForCell(c, out tempResult))
			{
				return true;
			}
			List<Thing> list = Find.ThingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.passability == Traversability.Impassable)
				{
					return GenTemperature.TryGetAirTemperatureAroundThing(list[i], out tempResult);
				}
			}
			return false;
		}

		public static bool TryGetDirectAirTemperatureForCell(IntVec3 c, out float temperature)
		{
			if (!c.InBounds())
			{
				temperature = 21f;
				return false;
			}
			if (c.Impassable())
			{
				temperature = 21f;
				return false;
			}
			Room room = RoomQuery.RoomAt(c);
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
				if (list[i].InBounds() && GenTemperature.TryGetDirectAirTemperatureForCell(list[i], out num3))
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

		private static float OutdoorTemperatureAt(int absTick)
		{
			return GenTemperature.CalculateOutdoorTemperatureAtWorldCoords(absTick, Find.Map.WorldCoords, true);
		}

		private static float CalculateOutdoorTemperatureAtWorldCoords(int absTick, IntVec2 worldCoords, bool includeDailyVariations)
		{
			if (absTick == 0)
			{
				absTick = 1;
			}
			WorldSquare worldSquare = Find.World.grid.Get(worldCoords);
			float num = worldSquare.temperature + GenTemperature.OffsetFromSeasonCycle(absTick, worldCoords);
			if (includeDailyVariations)
			{
				num += GenTemperature.OffsetFromDailyRandomVariation(absTick) + GenTemperature.OffsetFromSunCycle(absTick);
			}
			return num;
		}

		private static float OffsetFromDailyRandomVariation(int absTick)
		{
			return (float)GenTemperature.DailyVariationPerlin.GetValue((double)absTick, 0.0, 0.0) * 7f;
		}

		private static float OffsetFromSunCycle(int absTick)
		{
			float num = (float)(absTick % 60000) / 60000f;
			float f = 6.28318548f * (num + 0.32f);
			return Mathf.Cos(f) * 7f;
		}

		private static float OffsetFromSeasonCycle(int absTick, IntVec2 worldCoords)
		{
			float num = (float)(absTick / 60000 % 60) / 60f;
			float f = 6.28318548f * num;
			return Mathf.Cos(f) * -GenTemperature.SeasonalShiftAmplitudeAt(worldCoords);
		}

		private static float GetTemperatureFromSeasonAtWorldCoords(int absTick, IntVec2 worldCoords)
		{
			if (absTick == 0)
			{
				absTick = 1;
			}
			WorldSquare worldSquare = Find.World.grid.Get(worldCoords);
			return worldSquare.temperature + GenTemperature.OffsetFromSeasonCycle(absTick, worldCoords);
		}

		private static float SeasonalShiftAmplitudeAt(IntVec2 worldCoords)
		{
			return TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.World.DistanceFromEquatorNormalized(worldCoords));
		}

		private static void RebuildMonthlyTemperatureAverages()
		{
			if (GenTemperature.cachedMonthlyTempAverages == null)
			{
				GenTemperature.cachedMonthlyTempAverages = new float[12];
			}
			for (int i = 0; i < 12; i++)
			{
				GenTemperature.cachedMonthlyTempAverages[i] = GenTemperature.AverageTemperatureAtWorldCoordsForMonth(Find.Map.WorldCoords, (Month)i);
			}
		}

		private static float AverageTemperatureForMonth(Month month)
		{
			if (GenTemperature.cachedMonthlyTempAverages == null)
			{
				GenTemperature.RebuildMonthlyTemperatureAverages();
			}
			return GenTemperature.cachedMonthlyTempAverages[(int)month];
		}

		public static Month EarliestMonthInTemperatureRange(float minTemp, float maxTemp)
		{
			for (int i = 0; i < 12; i++)
			{
				float num = GenTemperature.AverageTemperatureForMonth((Month)i);
				if (num >= minTemp && num <= maxTemp)
				{
					return (Month)i;
				}
			}
			return Month.Undefined;
		}

		public static bool LocalSeasonsAreMeaningful()
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < 12; i++)
			{
				float num = GenTemperature.AverageTemperatureForMonth((Month)i);
				if (num > 0f)
				{
					flag2 = true;
				}
				if (num < 0f)
				{
					flag = true;
				}
			}
			return flag2 && flag;
		}

		public static List<Month> MonthsInTemperatureRange(float minTemp, float maxTemp)
		{
			return GenTemperature.MonthsInTemperatureRange(Find.Map.WorldCoords, minTemp, maxTemp);
		}

		public static List<Month> MonthsInTemperatureRange(IntVec2 worldCoords, float minTemp, float maxTemp)
		{
			List<Month> months = new List<Month>();
			for (int i = 0; i < 12; i++)
			{
				float num = GenTemperature.AverageTemperatureAtWorldCoordsForMonth(worldCoords, (Month)i);
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

		public static bool PushHeat(IntVec3 c, float energy)
		{
			Room room = c.GetRoom();
			if (room != null)
			{
				return room.PushHeat(energy);
			}
			GenTemperature.neighRooms.Clear();
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = c + GenAdj.AdjacentCells[i];
				if (intVec.InBounds())
				{
					room = intVec.GetRoom();
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
				GenTemperature.PushHeat(t.Position, energy);
			}
			else if (GenAdj.TryFindRandomWalkableAdjacentCell8Way(t, out c))
			{
				GenTemperature.PushHeat(c, energy);
			}
		}

		public static float ControlTemperatureTempChange(IntVec3 cell, float energyLimit, float targetTemperature)
		{
			Room room = RoomQuery.RoomAt(cell);
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

		public static void EqualizeTemperaturesThroughBuilding(Building b, float rate)
		{
			Room[] array = new Room[4];
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = b.Position + GenAdj.CardinalDirections[i];
				if (intVec.InBounds())
				{
					Room room = intVec.GetRoom();
					if (room != null)
					{
						num2 += room.Temperature;
						array[num] = room;
						num++;
					}
				}
			}
			if (num == 0)
			{
				return;
			}
			float num3 = num2 / (float)num;
			Room room2 = b.Position.GetRoom();
			if (room2 != null)
			{
				room2.Temperature = num3;
			}
			for (int j = 0; j < num; j++)
			{
				float temperature = array[j].Temperature;
				float num4 = num3 - temperature;
				float num5 = num4 * rate;
				array[j].PushHeat(num5);
				if (num5 > 0f && array[j].Temperature > num3)
				{
					array[j].Temperature = num3;
				}
				if (num5 < 0f && array[j].Temperature < num3)
				{
					array[j].Temperature = num3;
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

		public static bool FactionOwnsRoomInTemperatureRange(Faction faction, FloatRange tempRange)
		{
			if (faction == Faction.OfPlayer)
			{
				List<Room> allRooms = Find.RegionGrid.allRooms;
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

		public static void DebugLogTemps()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("-----Temperature for each hour this day------");
			stringBuilder.AppendLine("Hour    Temp    SunEffect");
			int num = Find.TickManager.TicksAbs - Find.TickManager.TicksAbs % 60000;
			for (int i = 0; i < 24; i++)
			{
				int absTick = num + i * 2500;
				stringBuilder.Append(i.ToString().PadRight(5));
				stringBuilder.Append(GenTemperature.OutdoorTemperatureAt(absTick).ToString("F2").PadRight(8));
				stringBuilder.Append(GenTemperature.OffsetFromSunCycle(absTick).ToString("F2"));
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("-----Temperature for each month this year------");
			for (int j = 0; j < 12; j++)
			{
				float num2 = GenTemperature.AverageTemperatureForMonth((Month)j);
				stringBuilder.AppendLine(((Month)j).ToString() + " " + num2.ToString("F2"));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("-----Temperature for each day this year------");
			stringBuilder.AppendLine("World square avg: " + Find.Map.WorldSquare.temperature + "°C");
			stringBuilder.AppendLine("Seasonal shift: " + GenTemperature.SeasonalShiftAmplitudeAt(Find.Map.WorldCoords));
			stringBuilder.AppendLine("Equatorial distance: " + Find.World.DistanceFromEquatorNormalized(Find.Map.WorldCoords));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Day  Lo   Hi   AvgDailyTemp RandomDailyVariation");
			for (int k = 0; k < 180; k++)
			{
				int absTick2 = (int)((float)(k * 60000) + 15000f);
				int absTick3 = (int)((float)(k * 60000) + 45000f);
				stringBuilder.Append(k.ToString().PadRight(8));
				stringBuilder.Append(GenTemperature.OutdoorTemperatureAt(absTick2).ToString("F2").PadRight(11));
				stringBuilder.Append(GenTemperature.OutdoorTemperatureAt(absTick3).ToString("F2").PadRight(11));
				stringBuilder.Append(GenTemperature.OffsetFromSeasonCycle(absTick3, Find.Map.WorldCoords).ToString("F2").PadRight(11));
				stringBuilder.Append(GenTemperature.OffsetFromDailyRandomVariation(absTick3).ToString("F2"));
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
