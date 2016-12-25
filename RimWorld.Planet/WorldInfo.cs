using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldInfo : IExposable
	{
		public string name = "DefaultWorldName";

		public float planetCoverage;

		public string seedString = "SeedError";

		public OverallRainfall overallRainfall = OverallRainfall.Normal;

		public OverallTemperature overallTemperature = OverallTemperature.Normal;

		public IntVec3 initialMapSize = new IntVec3(250, 1, 250);

		public string FileNameNoExtension
		{
			get
			{
				return GenText.CapitalizedNoSpaces(this.name);
			}
		}

		public int Seed
		{
			get
			{
				return GenText.StableStringHash(this.seedString);
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.name, "name", null, false);
			Scribe_Values.LookValue<float>(ref this.planetCoverage, "planetCoverage", 0f, false);
			Scribe_Values.LookValue<string>(ref this.seedString, "seedString", null, false);
			Scribe_Values.LookValue<OverallRainfall>(ref this.overallRainfall, "overallRainfall", OverallRainfall.AlmostNone, false);
			Scribe_Values.LookValue<OverallTemperature>(ref this.overallTemperature, "overallTemperature", OverallTemperature.VeryCold, false);
			Scribe_Values.LookValue<IntVec3>(ref this.initialMapSize, "initialMapSize", default(IntVec3), false);
		}
	}
}
