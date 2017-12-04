using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MurderousRage : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_MurderousRage mentalState_MurderousRage = pawn.MentalState as MentalState_MurderousRage;
			if (mentalState_MurderousRage == null || mentalState_MurderousRage.target == null || !pawn.CanReach(mentalState_MurderousRage.target, PathEndMode.Touch, Danger.Deadly, true, TraverseMode.ByPawn))
			{
				return null;
			}
			return new Job(JobDefOf.AttackMelee, mentalState_MurderousRage.target)
			{
				canBash = true,
				killIncappedTarget = true
			};
		}
	}
}
