using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Haul : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t);
			Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling(), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null);
			if (thing != null)
			{
				return HaulAIUtility.HaulToStorageJob(pawn, thing);
			}
			return null;
		}
	}
}
