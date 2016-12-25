using System;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldGenerator
	{
		public const float DefaultPlanetCoverage = 0.3f;

		public const OverallRainfall DefaultOverallRainfall = OverallRainfall.Normal;

		public const OverallTemperature DefaultOverallTemperature = OverallTemperature.Normal;

		public static World GenerateWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall, OverallTemperature overallTemperature)
		{
			Rand.Seed = (GenText.StableStringHash(seedString) ^ 4323276);
			Current.CreatingWorld = new World();
			Current.CreatingWorld.info.planetCoverage = planetCoverage;
			Current.CreatingWorld.info.seedString = seedString;
			Current.CreatingWorld.info.overallRainfall = overallRainfall;
			Current.CreatingWorld.info.overallTemperature = overallTemperature;
			Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, null, false);
			WorldGenerator_Grid.GenerateGridIntoWorld(seedString);
			Current.CreatingWorld.ConstructComponents();
			FactionGenerator.GenerateFactionsIntoWorld(seedString);
			Current.CreatingWorld.FinalizeInit();
			World creatingWorld = Current.CreatingWorld;
			Current.CreatingWorld = null;
			return creatingWorld;
		}
	}
}
