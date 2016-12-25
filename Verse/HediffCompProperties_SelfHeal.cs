using System;

namespace Verse
{
	public class HediffCompProperties_SelfHeal : HediffCompProperties
	{
		public int healIntervalTicksStanding = 50;

		public HediffCompProperties_SelfHeal()
		{
			this.compClass = typeof(HediffComp_SelfHeal);
		}
	}
}
