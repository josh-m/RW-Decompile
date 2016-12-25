using System;

namespace Verse
{
	public class CompProperties_Lifespan : CompProperties
	{
		public int lifespanTicks = 100;

		public CompProperties_Lifespan()
		{
			this.compClass = typeof(CompLifespan);
		}
	}
}
