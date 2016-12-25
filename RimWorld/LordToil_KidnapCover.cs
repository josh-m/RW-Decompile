using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_KidnapCover : LordToil_DoOpportunisticTaskOrCover
	{
		protected override DutyDef DutyDef
		{
			get
			{
				return DutyDefOf.Kidnap;
			}
		}

		protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
		{
			Pawn pawn2;
			bool result = KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 8f, out pawn2, alreadyTakenTargets);
			target = pawn2;
			return result;
		}
	}
}
