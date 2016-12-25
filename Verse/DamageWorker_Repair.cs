using System;

namespace Verse
{
	public class DamageWorker_Repair : DamageWorker
	{
		public override float Apply(DamageInfo dinfo, Thing thing)
		{
			if (!thing.def.useHitPoints)
			{
				return 0f;
			}
			thing.HitPoints += dinfo.Amount;
			if (thing.HitPoints > thing.MaxHitPoints)
			{
				thing.HitPoints = thing.MaxHitPoints;
			}
			return 0f;
		}
	}
}
