using System;

namespace Verse
{
	public class HediffCompProperties_Disappears : HediffCompProperties
	{
		public IntRange disappearsAfterTicks = default(IntRange);

		public HediffCompProperties_Disappears()
		{
			this.compClass = typeof(HediffComp_Disappears);
		}
	}
}
