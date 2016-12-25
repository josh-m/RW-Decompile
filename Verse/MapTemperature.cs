using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using Verse.Noise;

namespace Verse
{
	public class MapTemperature
	{
		private const int CachedTempUpdateInterval = 60;

		private Map map;

		private float cachedOutdoorTemp = -3.40282347E+38f;

		private float cachedSeasonalTemp = -3.40282347E+38f;

		private float[] cachedMonthlyTempAverages;

		private Perlin dailyVariationPerlinCached;

		private HashSet<int> fastProcessedRoomIDs = new HashSet<int>();

		public float OutdoorTemp
		{
			get
			{
				if (this.cachedOutdoorTemp == -3.40282347E+38f)
				{
					this.UpdateCachedData();
				}
				return this.cachedOutdoorTemp;
			}
		}

		public float SeasonalTemp
		{
			get
			{
				if (this.cachedSeasonalTemp == -3.40282347E+38f)
				{
					this.UpdateCachedData();
				}
				return this.cachedSeasonalTemp;
			}
		}

		private Perlin DailyVariationPerlin
		{
			get
			{
				if (this.dailyVariationPerlinCached == null)
				{
					int seed = Gen.HashCombineInt(this.map.Tile, 199372327);
					this.dailyVariationPerlinCached = new Perlin(4.9999998736893758E-06, 2.0, 0.5, 3, seed, QualityMode.Medium);
				}
				return this.dailyVariationPerlinCached;
			}
		}

		public MapTemperature(Map map)
		{
			this.map = map;
		}

		public void MapTemperatureTick()
		{
			if (Find.TickManager.TicksGame % 60 == 4)
			{
				this.UpdateCachedData();
			}
			if (Find.TickManager.TicksGame % 120 == 7)
			{
				this.fastProcessedRoomIDs.Clear();
				foreach (Region current in this.map.regionGrid.AllRegions)
				{
					if (current.Room != null && !this.fastProcessedRoomIDs.Contains(current.Room.ID))
					{
						current.Room.TempTracker.EqualizeTemperature();
						this.fastProcessedRoomIDs.Add(current.Room.ID);
					}
				}
			}
		}

		public void UpdateCachedData()
		{
			this.cachedOutdoorTemp = this.OutdoorTemperatureAt(Find.TickManager.TicksAbs);
			this.cachedOutdoorTemp += this.map.mapConditionManager.AggregateTemperatureOffset();
			this.cachedSeasonalTemp = this.CalculateOutdoorTemperatureAtTile(Find.TickManager.TicksAbs, this.map.Tile, false);
		}

		public bool SeasonAcceptableFor(ThingDef animalRace)
		{
			return this.SeasonalTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) && this.SeasonalTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
		}

		public bool OutdoorTemperatureAcceptableFor(ThingDef animalRace)
		{
			return this.OutdoorTemp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) && this.OutdoorTemp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
		}

		public bool SeasonAndOutdoorTemperatureAcceptableFor(ThingDef animalRace)
		{
			return this.SeasonAcceptableFor(animalRace) && this.OutdoorTemperatureAcceptableFor(animalRace);
		}

		private float OutdoorTemperatureAt(int absTick)
		{
			return this.CalculateOutdoorTemperatureAtTile(absTick, this.map.Tile, true);
		}

		private float CalculateOutdoorTemperatureAtTile(int absTick, int tile, bool includeDailyVariations)
		{
			if (absTick == 0)
			{
				absTick = 1;
			}
			Tile tile2 = Find.WorldGrid[tile];
			float num = tile2.temperature + GenTemperature.OffsetFromSeasonCycle(absTick, tile);
			if (includeDailyVariations)
			{
				num += this.OffsetFromDailyRandomVariation(absTick) + GenTemperature.OffsetFromSunCycle(absTick, tile);
			}
			return num;
		}

		private float OffsetFromDailyRandomVariation(int absTick)
		{
			return (float)this.DailyVariationPerlin.GetValue((double)absTick, 0.0, 0.0) * 7f;
		}

		private void RebuildMonthlyTemperatureAverages()
		{
			if (this.cachedMonthlyTempAverages == null)
			{
				this.cachedMonthlyTempAverages = new float[12];
			}
			for (int i = 0; i < 12; i++)
			{
				this.cachedMonthlyTempAverages[i] = GenTemperature.AverageTemperatureAtTileForMonth(this.map.Tile, (Month)i);
			}
		}

		private float AverageTemperatureForMonth(Month month)
		{
			if (this.cachedMonthlyTempAverages == null)
			{
				this.RebuildMonthlyTemperatureAverages();
			}
			return this.cachedMonthlyTempAverages[(int)month];
		}

		public Month EarliestMonthInTemperatureRange(float minTemp, float maxTemp)
		{
			for (int i = 0; i < 12; i++)
			{
				float num = this.AverageTemperatureForMonth((Month)i);
				if (num >= minTemp && num <= maxTemp)
				{
					return (Month)i;
				}
			}
			return Month.Undefined;
		}

		public bool LocalSeasonsAreMeaningful()
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < 12; i++)
			{
				float num = this.AverageTemperatureForMonth((Month)i);
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

		public void DebugLogTemps()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("-----Temperature for each hour this day------");
			stringBuilder.AppendLine("Hour    Temp    SunEffect");
			int num = Find.TickManager.TicksAbs - Find.TickManager.TicksAbs % 60000;
			for (int i = 0; i < 24; i++)
			{
				int absTick = num + i * 2500;
				stringBuilder.Append(i.ToString().PadRight(5));
				stringBuilder.Append(this.OutdoorTemperatureAt(absTick).ToString("F2").PadRight(8));
				stringBuilder.Append(GenTemperature.OffsetFromSunCycle(absTick, this.map.Tile).ToString("F2"));
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("-----Temperature for each month this year------");
			for (int j = 0; j < 12; j++)
			{
				float num2 = this.AverageTemperatureForMonth((Month)j);
				stringBuilder.AppendLine(((Month)j).ToString() + " " + num2.ToString("F2"));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("-----Temperature for each day this year------");
			stringBuilder.AppendLine("Tile avg: " + this.map.TileInfo.temperature + "°C");
			stringBuilder.AppendLine("Seasonal shift: " + GenTemperature.SeasonalShiftAmplitudeAt(this.map.Tile));
			stringBuilder.AppendLine("Equatorial distance: " + Find.WorldGrid.DistanceFromEquatorNormalized(this.map.Tile));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Day  Lo   Hi   AvgDailyTemp RandomDailyVariation");
			for (int k = 0; k < 180; k++)
			{
				int absTick2 = (int)((float)(k * 60000) + 15000f);
				int absTick3 = (int)((float)(k * 60000) + 45000f);
				stringBuilder.Append(k.ToString().PadRight(8));
				stringBuilder.Append(this.OutdoorTemperatureAt(absTick2).ToString("F2").PadRight(11));
				stringBuilder.Append(this.OutdoorTemperatureAt(absTick3).ToString("F2").PadRight(11));
				stringBuilder.Append(GenTemperature.OffsetFromSeasonCycle(absTick3, this.map.Tile).ToString("F2").PadRight(11));
				stringBuilder.Append(this.OffsetFromDailyRandomVariation(absTick3).ToString("F2"));
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
