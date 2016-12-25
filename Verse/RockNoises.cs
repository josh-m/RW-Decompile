using System;
using System.Collections.Generic;
using Verse.Noise;

namespace Verse
{
	public static class RockNoises
	{
		public class RockNoise
		{
			public ThingDef rockDef;

			public ModuleBase noise;
		}

		private const float RockNoiseFreq = 0.005f;

		public static List<RockNoises.RockNoise> rockNoises;

		public static void Init(Map map)
		{
			RockNoises.rockNoises = new List<RockNoises.RockNoise>();
			foreach (ThingDef current in Find.World.NaturalRockTypesIn(map.Tile))
			{
				RockNoises.RockNoise rockNoise = new RockNoises.RockNoise();
				rockNoise.rockDef = current;
				rockNoise.noise = new Perlin(0.004999999888241291, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.Medium);
				RockNoises.rockNoises.Add(rockNoise);
				NoiseDebugUI.StoreNoiseRender(rockNoise.noise, rockNoise.rockDef + " score", map.Size.ToIntVec2);
			}
		}

		public static void Reset()
		{
			RockNoises.rockNoises = null;
		}
	}
}
