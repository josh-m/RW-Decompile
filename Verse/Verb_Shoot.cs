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
				if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
				{
					if (this.currentTarget.Thing.HostileTo(this.caster))
					{
						xp = 240f;
					}
					else
					{
						xp = 50f;
					}
				}
				base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp);
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
