using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_LongRangeMineralScanner : CompProperties
	{
		public float radius = 30f;

		public float mtbDays = 30f;

		public CompProperties_LongRangeMineralScanner()
		{
			this.compClass = typeof(CompLongRangeMineralScanner);
		}
	}
}
