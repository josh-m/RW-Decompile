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
			Thing arg_4D_0 = fire;
			DamageDef extinguish = DamageDefOf.Extinguish;
			int amount = 32;
			Thing caster = this.caster;
			arg_4D_0.TakeDamage(new DamageInfo(extinguish, amount, -1f, caster, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
			casterPawn.Drawer.Notify_MeleeAttackOn(fire);
			return true;
		}
	}
}
