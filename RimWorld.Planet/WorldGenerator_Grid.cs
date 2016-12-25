using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public static class WorldGenerator_Grid
	{
		private const float ElevationFrequencyMicro = 0.035f;

		private const float ElevationFrequencyMacro = 0.012f;

		private const float ElevationMacroFactorFrequency = 0.12f;

		private const float ElevationContinentsFrequency = 0.01f;

		private const float MountainLinesFrequency = 0.04f;

		private const float MountainLinesHolesFrequency = 0.1f;

		private const float HillsPatchesFrequencyMicro = 0.19f;

		private const float HillsPatchesFrequencyMacro = 0.032f;

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

		private static ModuleBase noiseMountainLines;

		private static ModuleBase noiseHillsPatchesMicro;

		private static ModuleBase noiseHillsPatchesMacro;

		private static readonly FloatRange ElevationRange = new FloatRange(-500f, 5000f);

		private static readonly SimpleCurve AvgTempByLatitudeCurve = new SimpleCurve
		{
			new CurvePoint(0f, 30f),
			new CurvePoint(0.1f, 29f),
			new CurvePoint(0.5f, 7f),
			new CurvePoint(1f, -37f)
		};

		private static float FreqMultiplier
		{
			get
			{
				return 1f;
			}
		}

		public static void GenerateGridIntoWorld(string seedString)
		{
			Rand.Seed = GenText.StableStringHash(seedString);
			Find.World.grid = new WorldGrid();
			Find.World.pathGrid = new WorldPathGrid();
			NoiseDebugUI.ClearPlanetNoises();
			WorldGenerator_Grid.SetupElevationNoise();
			WorldGenerator_Grid.SetupTemperatureOffsetNoise();
			WorldGenerator_Grid.SetupRainfallNoise();
			WorldGenerator_Grid.SetupHillinessNoise();
			List<Tile> tiles = Find.WorldGrid.tiles;
			tiles.Clear();
			int tilesCount = Find.WorldGrid.TilesCount;
			for (int i = 0; i < tilesCount; i++)
			{
				Tile item = WorldGenerator_Grid.GenerateTileFor(i);
				tiles.Add(item);
			}
			Rand.RandomizeSeedFromTime();
		}

		private static void SetupElevationNoise()
		{
			float freqMultiplier = WorldGenerator_Grid.FreqMultiplier;
			ModuleBase lhs = new Perlin((double)(0.035f * freqMultiplier), 2.0, 0.40000000596046448, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase = new RidgedMultifractal((double)(0.012f * freqMultiplier), 2.0, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase2 = new Perlin((double)(0.12f * freqMultiplier), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase3 = new Perlin((double)(0.01f * freqMultiplier), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.High);
			float num;
			if (Find.World.PlanetCoverage < 0.55f)
			{
				ModuleBase moduleBase4 = new DistanceFromPlanetViewCenter(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, true);
				moduleBase4 = new ScaleBias(2.0, -1.0, moduleBase4);
				moduleBase3 = new Blend(moduleBase3, moduleBase4, new Const(0.40000000596046448));
				num = Rand.Range(-0.4f, -0.35f);
			}
			else
			{
				num = Rand.Range(0.15f, 0.25f);
			}
			NoiseDebugUI.StorePlanetNoise(moduleBase3, "elevContinents");
			moduleBase2 = new ScaleBias(0.5, 0.5, moduleBase2);
			moduleBase = new Multiply(moduleBase, moduleBase2);
			float num2 = Rand.Range(0.4f, 0.6f);
			WorldGenerator_Grid.noiseElevation = new Blend(lhs, moduleBase, new Const((double)num2));
			WorldGenerator_Grid.noiseElevation = new Blend(WorldGenerator_Grid.noiseElevation, moduleBase3, new Const((double)num));
			WorldGenerator_Grid.noiseElevation = new ScaleBias(0.5, 0.5, WorldGenerator_Grid.noiseElevation);
			WorldGenerator_Grid.noiseElevation = new Power(WorldGenerator_Grid.noiseElevation, new Const(3.0));
			NoiseDebugUI.StorePlanetNoise(WorldGenerator_Grid.noiseElevation, "noiseElevation");
			WorldGenerator_Grid.noiseElevation = new ScaleBias((double)WorldGenerator_Grid.ElevationRange.Span, (double)WorldGenerator_Grid.ElevationRange.min, WorldGenerator_Grid.noiseElevation);
		}

		private static void SetupTemperatureOffsetNoise()
		{
			float freqMultiplier = WorldGenerator_Grid.FreqMultiplier;
			WorldGenerator_Grid.noiseTemperatureOffset = new Perlin((double)(0.018f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			WorldGenerator_Grid.noiseTemperatureOffset = new Multiply(WorldGenerator_Grid.noiseTemperatureOffset, new Const(4.0));
		}

		private static void SetupRainfallNoise()
		{
			float freqMultiplier = WorldGenerator_Grid.FreqMultiplier;
			ModuleBase moduleBase = new Perlin((double)(0.015f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			NoiseDebugUI.StorePlanetNoise(moduleBase, "basePerlin");
			ModuleBase moduleBase2 = new AbsLatitudeCurve(new SimpleCurve
			{
				{
					0f,
					1.12f
				},
				{
					25f,
					0.94f
				},
				{
					45f,
					0.7f
				},
				{
					70f,
					0.3f
				},
				{
					80f,
					0.05f
				},
				{
					90f,
					0.05f
				}
			}, 100f);
			NoiseDebugUI.StorePlanetNoise(moduleBase2, "latCurve");
			WorldGenerator_Grid.noiseRainfall = new Multiply(moduleBase, moduleBase2);
			float num = 0.000222222225f;
			float num2 = -500f * num;
			ModuleBase moduleBase3 = new ScaleBias((double)num, (double)num2, WorldGenerator_Grid.noiseElevation);
			moduleBase3 = new ScaleBias(-1.0, 1.0, moduleBase3);
			moduleBase3 = new Clamp(0.0, 1.0, moduleBase3);
			NoiseDebugUI.StorePlanetNoise(moduleBase3, "elevationRainfallEffect");
			WorldGenerator_Grid.noiseRainfall = new Multiply(WorldGenerator_Grid.noiseRainfall, moduleBase3);
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
			NoiseDebugUI.StorePlanetNoise(WorldGenerator_Grid.noiseRainfall, "noiseRainfall before mm");
			WorldGenerator_Grid.noiseRainfall = new ScaleBias(4000.0, 0.0, WorldGenerator_Grid.noiseRainfall);
			SimpleCurve rainfallCurve = Find.World.info.overallRainfall.GetRainfallCurve();
			if (rainfallCurve != null)
			{
				WorldGenerator_Grid.noiseRainfall = new CurveSimple(WorldGenerator_Grid.noiseRainfall, rainfallCurve);
			}
		}

		private static void SetupHillinessNoise()
		{
			float freqMultiplier = WorldGenerator_Grid.FreqMultiplier;
			WorldGenerator_Grid.noiseMountainLines = new Perlin((double)(0.04f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase = new Perlin((double)(0.1f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			WorldGenerator_Grid.noiseMountainLines = new Abs(WorldGenerator_Grid.noiseMountainLines);
			WorldGenerator_Grid.noiseMountainLines = new OneMinus(WorldGenerator_Grid.noiseMountainLines);
			moduleBase = new Filter(moduleBase, -0.3f, 1f);
			WorldGenerator_Grid.noiseMountainLines = new Multiply(WorldGenerator_Grid.noiseMountainLines, moduleBase);
			WorldGenerator_Grid.noiseMountainLines = new OneMinus(WorldGenerator_Grid.noiseMountainLines);
			NoiseDebugUI.StorePlanetNoise(WorldGenerator_Grid.noiseMountainLines, "noiseMountainLines");
			WorldGenerator_Grid.noiseHillsPatchesMacro = new Perlin((double)(0.032f * freqMultiplier), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.Medium);
			WorldGenerator_Grid.noiseHillsPatchesMicro = new Perlin((double)(0.19f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
		}

		private static Tile GenerateTileFor(int tileID)
		{
			Tile tile = new Tile();
			Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
			tile.elevation = WorldGenerator_Grid.noiseElevation.GetValue(tileCenter);
			float value = WorldGenerator_Grid.noiseMountainLines.GetValue(tileCenter);
			if (value > 0.235f || tile.elevation <= 0f)
			{
				if (tile.elevation > 0f && WorldGenerator_Grid.noiseHillsPatchesMicro.GetValue(tileCenter) > 0.46f && WorldGenerator_Grid.noiseHillsPatchesMacro.GetValue(tileCenter) > -0.3f)
				{
					if (Rand.Bool)
					{
						tile.hilliness = Hilliness.SmallHills;
					}
					else
					{
						tile.hilliness = Hilliness.LargeHills;
					}
				}
				else
				{
					tile.hilliness = Hilliness.Flat;
				}
			}
			else if (value > 0.12f)
			{
				switch (Rand.Range(0, 4))
				{
				case 0:
					tile.hilliness = Hilliness.Flat;
					break;
				case 1:
					tile.hilliness = Hilliness.SmallHills;
					break;
				case 2:
					tile.hilliness = Hilliness.LargeHills;
					break;
				case 3:
					tile.hilliness = Hilliness.Mountainous;
					break;
				}
			}
			else if (value > 0.0363f)
			{
				tile.hilliness = Hilliness.Mountainous;
			}
			else
			{
				tile.hilliness = Hilliness.Impassable;
			}
			float num = WorldGenerator_Grid.BaseTemperatureAtLatitude(Find.WorldGrid.LongLatOf(tileID).y);
			num -= WorldGenerator_Grid.TemperatureReductionAtElevation(tile.elevation);
			num += WorldGenerator_Grid.noiseTemperatureOffset.GetValue(tileCenter);
			SimpleCurve temperatureCurve = Find.World.info.overallTemperature.GetTemperatureCurve();
			if (temperatureCurve != null)
			{
				num = temperatureCurve.Evaluate(num);
			}
			tile.temperature = num;
			tile.rainfall = WorldGenerator_Grid.noiseRainfall.GetValue(tileCenter);
			if (float.IsNaN(tile.rainfall))
			{
				float value2 = WorldGenerator_Grid.noiseRainfall.GetValue(tileCenter);
				Log.ErrorOnce(value2 + " rain bad at " + tileID, 694822);
			}
			tile.biome = WorldGenerator_Grid.BiomeFrom(tile);
			return tile;
		}

		private static BiomeDef BiomeFrom(Tile ws)
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
			float x = Mathf.Abs(lat) / 90f;
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
