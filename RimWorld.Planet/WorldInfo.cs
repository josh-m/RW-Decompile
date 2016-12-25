using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldInfo : IExposable
	{
		public string name = "DefaultWorldName";

		public IntVec2 size = IntVec2.Invalid;

		public string seedString = "SeedError";

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
			Scribe_Values.LookValue<IntVec2>(ref this.size, "size", default(IntVec2), false);
			Scribe_Values.LookValue<string>(ref this.seedString, "seedString", null, false);
		}
	}
}
