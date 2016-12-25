using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	internal class JobGiver_FightFiresNearPoint : ThinkNode_JobGiver
	{
		public float maxDistFromPoint = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_FightFiresNearPoint jobGiver_FightFiresNearPoint = (JobGiver_FightFiresNearPoint)base.DeepCopy(resolve);
			jobGiver_FightFiresNearPoint.maxDistFromPoint = this.maxDistFromPoint;
			return jobGiver_FightFiresNearPoint;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn2 = ((AttachableThing)t).parent as Pawn;
				return pawn2 == null && pawn.CanReserve(t, 1) && !pawn.story.WorkTagIsDisabled(WorkTags.Firefighting);
			};
			Thing thing = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), this.maxDistFromPoint, validator, null, -1, false);
			if (thing != null)
			{
				return new Job(JobDefOf.BeatFire, thing);
			}
			return null;
		}
	}
}
