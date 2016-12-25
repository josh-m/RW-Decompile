using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class DamageWorker_Flame : DamageWorker_AddInjury
	{
		public override float Apply(DamageInfo dinfo, Thing victim)
		{
			if (!dinfo.InstantOldInjury)
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			return base.Apply(dinfo, victim);
		}

		public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, bool canThrowMotes)
		{
			base.ExplosionAffectCell(explosion, c, damagedThings, canThrowMotes);
			if (this.def == DamageDefOf.Flame)
			{
				FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f));
			}
		}
	}
}
