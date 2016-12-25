using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public static class WorldGenerator_Grid
	{
		private const float OceanChancePerSide = 0.5f;

		private const float EdgeOceanSpan = 0.2f;

		private const float ElevationFrequencyMicro = 0.035f;

		private const float ElevationFrequencyMacro = 0.012f;

		private const float ElevationMacroFactorFrequency = 0.12f;

		private const float TemperatureOffsetFrequency = 0.018f;

		private const float TemperatureOffsetFactor = 4f;

		private const float ElevationTempReductionStartAlt = 250f;

		private const float ElevationTempReductionEndAlt = 5000f;

		private const float MaxElevationTempReduction = 40f;

		private const float RainfallOffsetFrequency = 0.013f;

		private const float RainfallPower = 1.5f;

		private const float RainfallFactor = 4000f;

		private const float RainfallStartFallAltitude = 500f;

		private const float RainfallFinishFallAltitude = 5000f;

		private const float FertilityTempMinimum = -15f;

		private const float FertilityTempOptimal = 30f;

		private const float FertilityTempMaximum = 50f;

		private static ModuleBase noiseElevation;

		private static ModuleBase noiseTemperatureOffset;

		private static ModuleBase noiseRainfall;

		private static readonly FloatRange ElevationRange = new FloatRange(-500f, 5000f);

		private static readonly SimpleCurve AvgTempByLatitudeCurve = new SimpleCurve
		{
			new CurvePoint(0f, 30f),
			new CurvePoint(0.1f, 29f),
			new CurvePoint(0.5f, 7f),
			new CurvePoint(1f, -37f)
		};

		public static void GenerateGridIntoWorld(string seedString)
		{
			Rand.Seed = GenText.StableStringHash(seedString);
			Find.World.grid = new WorldGrid();
			NoiseDebugUI.RenderSize = Find.World.Size;
			float num = 1f / ((float)Find.World.Size.x / 200f);
			ModuleBase lhs = new Perlin((double)(0.035f * num), 2.0, 0.40000000596046448, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase = new RidgedMultifractal((double)(0.012f * num), 2.0, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase2 = new Perlin((double)(0.12f * num), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase2 = new ScaleBias(0.5, 0.5, moduleBase2);
			moduleBase = new Multiply(moduleBase, moduleBase2);
			float num2 = Rand.Range(0.4f, 0.6f);
			WorldGenerator_Grid.noiseElevation = new Blend(lhs, moduleBase, new Const((double)num2));
			WorldGenerator_Grid.noiseElevation = new ScaleBias(0.5, 0.5, WorldGenerator_Grid.noiseElevation);
			WorldGenerator_Grid.noiseElevation = new Power(WorldGenerator_Grid.noiseElevation, new Const(3.0));
			for (int i = 0; i < 4; i++)
			{
				if (Rand.Value < 0.5f)
				{
					ModuleBase moduleBase3 = new DistFromAxis((float)Find.World.Size.x * 0.2f);
					moduleBase3 = new Clamp(0.0, 1.0, moduleBase3);
					if (i == 1)
					{
						moduleBase3 = new Translate((double)(-(double)Find.World.Size.x), 0.0, 0.0, moduleBase3);
					}
					else if (i == 2)
					{
						moduleBase3 = new Rotate(0.0, 90.0, 0.0, moduleBase3);
					}
					else if (i == 3)
					{
						moduleBase3 = new Rotate(0.0, 90.0, 0.0, moduleBase3);
						moduleBase3 = new Translate(0.0, 0.0, (double)(-(double)Find.World.Size.z), moduleBase3);
					}
					WorldGenerator_Grid.noiseElevation = new Multiply(WorldGenerator_Grid.noiseElevation, moduleBase3);
				}
			}
			WorldGenerator_Grid.noiseElevation = new ScaleBias((double)WorldGenerator_Grid.ElevationRange.Span, (double)WorldGenerator_Grid.ElevationRange.min, WorldGenerator_Grid.noiseElevation);
			WorldGenerator_Grid.noiseTemperatureOffset = new Perlin((double)(0.018f * num), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			WorldGenerator_Grid.noiseTemperatureOffset = new Multiply(WorldGenerator_Grid.noiseTemperatureOffset, new Const(4.0));
			ModuleBase moduleBase4 = new Perlin((double)(0.015f * num), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase4 = new ScaleBias(0.5, 0.5, moduleBase4);
			NoiseDebugUI.StoreNoiseRender(moduleBase4, "basePerlin");
			float num3 = 1f / Find.World.DegreesPerSquare;
			ModuleBase moduleBase5 = new CurveFromAxis(new SimpleCurve
			{
				{
					0f,
					1.12f
				},
				{
					25f * num3,
					0.94f
				},
				{
					45f * num3,
					0.7f
				},
				{
					70f * num3,
					0.3f
				},
				{
					80f * num3,
					0.05f
				}
			});
			moduleBase5 = new Rotate(0.0, 90.0, 0.0, moduleBase5);
			NoiseDebugUI.StoreNoiseRender(moduleBase5, "latCurve");
			WorldGenerator_Grid.noiseRainfall = new Multiply(moduleBase4, moduleBase5);
			float num4 = 0.000222222225f;
			float num5 = -500f * num4;
			ModuleBase moduleBase6 = new ScaleBias((double)num4, (double)num5, WorldGenerator_Grid.noiseElevation);
			moduleBase6 = new ScaleBias(-1.0, 1.0, moduleBase6);
			moduleBase6 = new Clamp(0.0, 1.0, moduleBase6);
			NoiseDebugUI.StoreNoiseRender(moduleBase6, "elevationRainfallEffect");
			WorldGenerator_Grid.noiseRainfall = new Multiply(WorldGenerator_Grid.noiseRainfall, moduleBase6);
			Func<double, double> processor = delegate(double val)
			{
				if (val < 0.0)
				{
					val = 0.0;
				}
				if (val < 0.12)
				{
					val = (val + 0.12) / 2.0;
					if (val < 0.03)
					{
						val = (val + 0.03) / 2.0;
					}
				}
				return val;
			};
			WorldGenerator_Grid.noiseRainfall = new Arbitrary(WorldGenerator_Grid.noiseRainfall, processor);
			WorldGenerator_Grid.noiseRainfall = new Power(WorldGenerator_Grid.noiseRainfall, new Const(1.5));
			WorldGenerator_Grid.noiseRainfall = new Clamp(0.0, 999.0, WorldGenerator_Grid.noiseRainfall);
			NoiseDebugUI.StoreNoiseRender(WorldGenerator_Grid.noiseRainfall, "noiseRainfall before mm");
			WorldGenerator_Grid.noiseRainfall = new ScaleBias(4000.0, 0.0, WorldGenerator_Grid.noiseRainfall);
			foreach (IntVec2 current in Find.World.AllSquares)
			{
				WorldSquare sqDef = WorldGenerator_Grid.GenerateWorldSquareFor(current);
				Find.World.grid.Set(current, sqDef);
			}
			Rand.RandomizeSeedFromTime();
		}

		private static WorldSquare GenerateWorldSquareFor(IntVec2 wc)
		{
			WorldSquare worldSquare = new WorldSquare();
			worldSquare.elevation = WorldGenerator_Grid.noiseElevation.GetValue(wc);
			if (worldSquare.elevation < 300f)
			{
				worldSquare.hilliness = Hilliness.Flat;
			}
			else if (worldSquare.elevation < 1500f)
			{
				switch (Rand.Range(0, 4))
				{
				case 0:
					worldSquare.hilliness = Hilliness.Flat;
					break;
				case 1:
					worldSquare.hilliness = Hilliness.SmallHills;
					break;
				case 2:
					worldSquare.hilliness = Hilliness.LargeHills;
					break;
				case 3:
					worldSquare.hilliness = Hilliness.Mountainous;
					break;
				}
			}
			else
			{
				worldSquare.hilliness = Hilliness.Mountainous;
			}
			float num = WorldGenerator_Grid.BaseTemperatureAtLatitude(Find.World.LongLatOf(wc).y);
			num -= WorldGenerator_Grid.TemperatureReductionAtElevation(worldSquare.elevation);
			num += WorldGenerator_Grid.noiseTemperatureOffset.GetValue(wc);
			worldSquare.temperature = num;
			worldSquare.rainfall = WorldGenerator_Grid.noiseRainfall.GetValue(wc);
			if (float.IsNaN(worldSquare.rainfall))
			{
				float value = WorldGenerator_Grid.noiseRainfall.GetValue(wc);
				Log.ErrorOnce(value + " rain bad at " + wc, 694822);
			}
			worldSquare.biome = WorldGenerator_Grid.BiomeFrom(worldSquare);
			return worldSquare;
		}

		private static BiomeDef BiomeFrom(WorldSquare ws)
		{
			List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
			BiomeDef biomeDef = null;
			float num = 0f;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				BiomeDef biomeDef2 = allDefsListForReading[i];
				if (biomeDef2.implemented)
				{
					float score = biomeDef2.Worker.GetScore(ws);
					if (score > num || biomeDef == null)
					{
						biomeDef = biomeDef2;
						num = score;
					}
				}
			}
			return biomeDef;
		}

		private static float FertilityFactorFromTemperature(float temp)
		{
			if (temp < -15f)
			{
				return 0f;
			}
			if (temp < 30f)
			{
				return Mathf.InverseLerp(-15f, 30f, temp);
			}
			if (temp < 50f)
			{
				return Mathf.InverseLerp(50f, 30f, temp);
			}
			return 0f;
		}

		private static float BaseTemperatureAtLatitude(float lat)
		{
			float x = lat / 90f;
			return WorldGenerator_Grid.AvgTempByLatitudeCurve.Evaluate(x);
		}

		private static float TemperatureReductionAtElevation(float elev)
		{
			if (elev < 250f)
			{
				return 0f;
			}
			float t = (elev - 250f) / 4750f;
			return Mathf.Lerp(0f, 40f, t);
		}
	}
}
