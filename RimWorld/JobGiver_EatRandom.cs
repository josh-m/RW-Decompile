using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_EatRandom : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.Downed)
			{
				return null;
			}
			Predicate<Thing> validator = (Thing t) => t.def.category == ThingCategory.Item && t.IngestibleNow && pawn.RaceProps.CanEverEat(t);
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 10f, validator, null, -1, false);
			if (thing == null)
			{
				return null;
			}
			return new Job(JobDefOf.Ingest, thing);
		}
	}
}
