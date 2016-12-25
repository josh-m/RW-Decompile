using System;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_Berserk : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Dead)
			{
				return;
			}
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
		}
	}
}
