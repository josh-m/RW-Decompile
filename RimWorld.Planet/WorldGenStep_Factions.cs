using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Factions : WorldGenStep
	{
		public override void GenerateFresh(string seed)
		{
			Rand.Seed = GenText.StableStringHash(seed);
			FactionGenerator.GenerateFactionsIntoWorld();
			Rand.RandomizeStateFromTime();
		}

		public override void GenerateFromScribe(string seed)
		{
		}
	}
}
