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
			fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 32, this.caster, null, null));
			casterPawn.Drawer.Notify_MeleeAttackOn(fire);
			return true;
		}
	}
}
