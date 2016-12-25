using System;
using Verse;

namespace RimWorld
{
	public class Verb_BeatFire : Verb
	{
		private const int DamageAmount = 32;

		public Verb_BeatFire()
		{
			this.verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.BeatFire);
		}

		protected override bool TryCastShot()
		{
			Fire fire = (Fire)this.currentTarget.Thing;
			Pawn casterPawn = base.CasterPawn;
			if (casterPawn.stances.FullBodyBusy)
			{
				return false;
			}
			Thing arg_46_0 = fire;
			Thing caster = this.caster;
			arg_46_0.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 32, -1f, caster, null, null));
			casterPawn.Drawer.Notify_MeleeAttackOn(fire);
			return true;
		}
	}
}
