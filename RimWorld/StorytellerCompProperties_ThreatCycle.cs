using System;

namespace RimWorld
{
	public class StorytellerCompProperties_ThreatCycle : StorytellerCompProperties
	{
		public float mtbDaysThreatSmall;

		public float mtbDaysThreatBig;

		public float threatCycleLength;

		public float minDaysBetweenThreatBigs;

		public StorytellerCompProperties_ThreatCycle()
		{
			this.compClass = typeof(StorytellerComp_ThreatCycle);
		}
	}
}
