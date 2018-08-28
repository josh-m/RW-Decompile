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
			Thing arg_56_0 = fire;
			DamageDef extinguish = DamageDefOf.Extinguish;
			float amount = 32f;
			Thing caster = this.caster;
			arg_56_0.TakeDamage(new DamageInfo(extinguish, amount, 0f, -1f, caster, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
			casterPawn.Drawer.Notify_MeleeAttackOn(fire);
			return true;
		}
	}
}
