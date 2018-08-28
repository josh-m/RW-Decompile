using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_LongRangeMineralScanner : CompProperties
	{
		public float radius = 30f;

		public float mtbDays = 9.2f;

		public float guaranteedToFindLumpAfterDaysWorking = 8f;

		public CompProperties_LongRangeMineralScanner()
		{
			this.compClass = typeof(CompLongRangeMineralScanner);
		}
	}
}
