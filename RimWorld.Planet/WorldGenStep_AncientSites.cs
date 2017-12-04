using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_AncientSites : WorldGenStep
	{
		public FloatRange ancientSitesPer100kTiles;

		public override void GenerateFresh(string seed)
		{
			Rand.Seed = GenText.StableStringHash(seed);
			this.GenerateAncientSites();
			Rand.RandomizeStateFromTime();
		}

		private void GenerateAncientSites()
		{
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * this.ancientSitesPer100kTiles.RandomInRange);
			for (int i = 0; i < num; i++)
			{
				Find.World.genData.ancientSites.Add(TileFinder.RandomFactionBaseTileFor(null, false, null));
			}
		}
	}
}
