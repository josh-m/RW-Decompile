using System;

namespace Verse
{
	public class HediffCompProperties_TendDuration : HediffCompProperties
	{
		public int tendDuration = -1;

		public bool tendAllAtOnce;

		public int disappearsAtTendedCount = -1;

		public float severityPerDayTended;

		[LoadAlias("labelTreatedWell")]
		public string labelTendedWell;

		[LoadAlias("labelTreatedWellInner")]
		public string labelTendedWellInner;

		[LoadAlias("labelSolidTreatedWell")]
		public string labelSolidTendedWell;

		public HediffCompProperties_TendDuration()
		{
			this.compClass = typeof(HediffComp_TendDuration);
		}
	}
}
