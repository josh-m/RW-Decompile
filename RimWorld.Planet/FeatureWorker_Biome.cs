using System;
using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_Biome : FeatureWorker_FloodFill
	{
		protected override bool IsRoot(int tile)
		{
			return this.def.rootBiomes.Contains(Find.WorldGrid[tile].biome);
		}

		protected override bool IsPossiblyAllowed(int tile)
		{
			return this.def.acceptableBiomes.Contains(Find.WorldGrid[tile].biome);
		}
	}
}
