using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal class JobGiver_FireStartingSpree : ThinkNode_JobGiver
	{
		private const float FireStartChance = 0.75f;

		private IntRange waitTicks = new IntRange(80, 140);

		private static List<Thing> potentialTargets = new List<Thing>();

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_FireStartingSpree jobGiver_FireStartingSpree = (JobGiver_FireStartingSpree)base.DeepCopy(resolve);
			jobGiver_FireStartingSpree.waitTicks = this.waitTicks;
			return jobGiver_FireStartingSpree;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.mindState.nextMoveOrderIsWait)
			{
				Job job = new Job(JobDefOf.WaitWander);
				job.expiryInterval = this.waitTicks.RandomInRange;
				pawn.mindState.nextMoveOrderIsWait = false;
				return job;
			}
			if (Rand.Value < 0.75f)
			{
				Thing thing = this.TryFindRandomIgniteTarget(pawn);
				if (thing != null)
				{
					pawn.mindState.nextMoveOrderIsWait = true;
					return new Job(JobDefOf.Ignite, thing);
				}
			}
			IntVec3 intVec = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 10f, null, Danger.Deadly);
			if (intVec.IsValid)
			{
				pawn.mindState.nextMoveOrderIsWait = true;
				pawn.Map.pawnDestinationManager.ReserveDestinationFor(pawn, intVec);
				return new Job(JobDefOf.GotoWander, intVec);
			}
			return null;
		}

		private Thing TryFindRandomIgniteTarget(Pawn pawn)
		{
			Region region;
			if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), (Region candidateRegion) => !candidateRegion.IsForbiddenEntirely(pawn), 100, out region))
			{
				return null;
			}
			JobGiver_FireStartingSpree.potentialTargets.Clear();
			List<Thing> allThings = region.ListerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				Thing thing = allThings[i];
				if ((thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Plant) && thing.FlammableNow && !thing.IsBurning() && !thing.OccupiedRect().Contains(pawn.Position))
				{
					JobGiver_FireStartingSpree.potentialTargets.Add(thing);
				}
			}
			if (JobGiver_FireStartingSpree.potentialTargets.NullOrEmpty<Thing>())
			{
				return null;
			}
			return JobGiver_FireStartingSpree.potentialTargets.RandomElement<Thing>();
		}
	}
}
