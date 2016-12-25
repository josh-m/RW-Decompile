using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_StealCover : LordToil_DoOpportunisticTaskOrCover
	{
		protected override DutyDef DutyDef
		{
			get
			{
				return DutyDefOf.Steal;
			}
		}

		protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
		{
			return StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 7f, out target, pawn, alreadyTakenTargets);
		}
	}
}
