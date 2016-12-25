using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_RescueNearby : ThinkNode_JobGiver
	{
		private const float MinDistFromEnemy = 25f;

		private float radius = 30f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_RescueNearby jobGiver_RescueNearby = (JobGiver_RescueNearby)base.DeepCopy(resolve);
			jobGiver_RescueNearby.radius = this.radius;
			return jobGiver_RescueNearby;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn3 = (Pawn)t;
				return pawn3.Downed && pawn3.Faction == pawn.Faction && !pawn3.InBed() && pawn.CanReserve(pawn3, 1) && !pawn3.IsForbidden(pawn) && !GenAI.EnemyIsNear(pawn3, 25f);
			};
			Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), this.radius, validator, null, -1, false);
			if (pawn2 == null)
			{
				return null;
			}
			Thing thing = RestUtility.FindBedFor(pawn2, pawn, pawn2.HostFaction == pawn.Faction, false, true);
			if (thing == null || !pawn2.CanReserve(thing, 1))
			{
				return null;
			}
			return new Job(JobDefOf.Rescue, pawn2, thing)
			{
				count = 1
			};
		}
	}
}
