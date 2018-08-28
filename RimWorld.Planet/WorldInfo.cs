using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldInfo : IExposable
	{
		public string name = "DefaultWorldName";

		public float planetCoverage;

		public string seedString = "SeedError";

		public int persistentRandomValue = Rand.Int;

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
			Scribe_Values.Look<string>(ref this.name, "name", null, false);
			Scribe_Values.Look<float>(ref this.planetCoverage, "planetCoverage", 0f, false);
			Scribe_Values.Look<string>(ref this.seedString, "seedString", null, false);
			Scribe_Values.Look<int>(ref this.persistentRandomValue, "persistentRandomValue", 0, false);
			Scribe_Values.Look<OverallRainfall>(ref this.overallRainfall, "overallRainfall", OverallRainfall.AlmostNone, false);
			Scribe_Values.Look<OverallTemperature>(ref this.overallTemperature, "overallTemperature", OverallTemperature.VeryCold, false);
			Scribe_Values.Look<IntVec3>(ref this.initialMapSize, "initialMapSize", default(IntVec3), false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.WorldInfoPostLoadInit(this);
			}
		}
	}
}
