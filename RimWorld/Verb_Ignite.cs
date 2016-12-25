using System;
using Verse;

namespace RimWorld
{
	public class Verb_Ignite : Verb
	{
		public Verb_Ignite()
		{
			this.verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.Ignite);
		}

		protected override bool TryCastShot()
		{
			Thing thing = this.currentTarget.Thing;
			Pawn casterPawn = base.CasterPawn;
			if (casterPawn.stances.FullBodyBusy)
			{
				return false;
			}
			FireUtility.TryStartFireIn(thing.OccupiedRect().ClosestCellTo(casterPawn.Position), casterPawn.Map, 0.3f);
			casterPawn.Drawer.Notify_MeleeAttackOn(thing);
			return true;
		}
	}
}
