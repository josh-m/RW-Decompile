using System;

namespace Verse
{
	public class HediffCompProperties_SeverityPerDay : HediffCompProperties
	{
		public float severityPerDay;

		public HediffCompProperties_SeverityPerDay()
		{
			this.compClass = typeof(HediffComp_SeverityPerDay);
		}
	}
}
