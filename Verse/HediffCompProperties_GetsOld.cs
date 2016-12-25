using System;

namespace Verse
{
	public class HediffCompProperties_GetsOld : HediffCompProperties
	{
		public float becomeOldChance = 1f;

		public string oldLabel;

		public string instantlyOldLabel;

		public HediffCompProperties_GetsOld()
		{
			this.compClass = typeof(HediffComp_GetsOld);
		}
	}
}
