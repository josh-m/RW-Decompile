using RimWorld;
using System;

namespace Verse
{
	public class Verb_Shoot : Verb_LaunchProjectile
	{
		protected override int ShotsPerBurst
		{
			get
			{
				return this.verbProps.burstShotCount;
			}
		}

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			if (base.CasterIsPawn && base.CasterPawn.skills != null)
			{
				float xp = 6f;
				Pawn pawn = this.currentTarget.Thing as Pawn;
				if (pawn != null && pawn.HostileTo(this.caster) && !pawn.Downed)
				{
					xp = 240f;
				}
				base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp, false);
			}
		}

		protected override bool TryCastShot()
		{
			bool flag = base.TryCastShot();
			if (flag && base.CasterIsPawn)
			{
				base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
			return flag;
		}
	}
}
