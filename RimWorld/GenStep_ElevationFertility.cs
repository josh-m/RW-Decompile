using RimWorld.Planet;
using System;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class GenStep_ElevationFertility : GenStep
	{
		private const float ElevationFreq = 0.021f;

		private const float FertilityFreq = 0.021f;

		private const float EdgeMountainSpan = 0.42f;

		public override void Generate()
		{
			NoiseRenderer.renderSize = new IntVec2(Find.Map.Size.x, Find.Map.Size.z);
			ModuleBase moduleBase = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev base");
			float num = 1f;
			switch (Find.MapWorldSquare.hilliness)
			{
			case Hilliness.Flat:
				num = MapGenTuning.ElevationFactorFlat;
				break;
			case Hilliness.SmallHills:
				num = MapGenTuning.ElevationFactorSmallHills;
				break;
			case Hilliness.LargeHills:
				num = MapGenTuning.ElevationFactorLargeHills;
				break;
			case Hilliness.Mountainous:
				num = MapGenTuning.ElevationFactorMountains;
				break;
			}
			moduleBase = new Multiply(moduleBase, new Const((double)num));
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev world-factored");
			if (Find.MapWorldSquare.hilliness == Hilliness.Mountainous)
			{
				ModuleBase moduleBase2 = new DistFromAxis((float)Find.Map.Size.x * 0.42f);
				moduleBase2 = new Clamp(0.0, 1.0, moduleBase2);
				moduleBase2 = new Invert(moduleBase2);
				moduleBase2 = new ScaleBias(1.0, 1.0, moduleBase2);
				Rot4 random;
				do
				{
					random = Rot4.Random;
				}
				while (random == Find.World.CoastDirectionAt(Find.GameInitData.startingCoords));
				if (random == Rot4.North)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
					moduleBase2 = new Translate(0.0, 0.0, (double)(-(double)Find.Map.Size.z), moduleBase2);
				}
				else if (random == Rot4.East)
				{
					moduleBase2 = new Translate((double)(-(double)Find.Map.Size.x), 0.0, 0.0, moduleBase2);
				}
				else if (random == Rot4.South)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
				}
				else if (random == Rot4.West)
				{
				}
				NoiseDebugUI.StoreNoiseRender(moduleBase2, "mountain");
				moduleBase = new Add(moduleBase, moduleBase2);
				NoiseDebugUI.StoreNoiseRender(moduleBase, "elev + mountain");
			}
			MapGenFloatGrid mapGenFloatGrid = MapGenerator.FloatGridNamed("Elevation");
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				MapGenFloatGrid mapGenFloatGrid2;
				MapGenFloatGrid expr_2BF = mapGenFloatGrid2 = mapGenFloatGrid;
				IntVec3 c;
				IntVec3 expr_2C4 = c = current;
				float num2 = mapGenFloatGrid2[c];
				expr_2BF[expr_2C4] = num2 + moduleBase.GetValue(current);
			}
			ModuleBase moduleBase3 = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			moduleBase3 = new ScaleBias(0.5, 0.5, moduleBase3);
			NoiseDebugUI.StoreNoiseRender(moduleBase3, "noiseFert base");
			MapGenFloatGrid mapGenFloatGrid3 = MapGenerator.FloatGridNamed("Fertility");
			foreach (IntVec3 current2 in Find.Map.AllCells)
			{
				mapGenFloatGrid3[current2] = moduleBase3.GetValue(current2);
			}
		}
	}
}
