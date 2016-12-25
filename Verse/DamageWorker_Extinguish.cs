using RimWorld;
using System;

namespace Verse
{
	public class DamageWorker_Extinguish : DamageWorker
	{
		public override float Apply(DamageInfo dinfo, Thing victim)
		{
			if (victim is Fire)
			{
				return base.Apply(dinfo, victim);
			}
			return 0f;
		}
	}
}
