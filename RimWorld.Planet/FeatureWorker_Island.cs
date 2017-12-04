using System;
using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_Island : FeatureWorker_FloodFill
	{
		protected override bool IsRoot(int tile)
		{
			BiomeDef biome = Find.WorldGrid[tile].biome;
			return biome != BiomeDefOf.Ocean && biome != BiomeDefOf.Lake;
		}

		protected override bool IsPossiblyAllowed(int tile)
		{
			return Find.WorldGrid[tile].biome == BiomeDefOf.Lake;
		}
	}
}
