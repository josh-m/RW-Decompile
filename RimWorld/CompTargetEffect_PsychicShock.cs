using System;
using Verse;

namespace RimWorld
{
	public class CompTargetEffect_PsychicShock : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Dead)
			{
				return;
			}
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn, null);
			pawn.health.AddHediff(hediff, null, null);
		}
	}
}
