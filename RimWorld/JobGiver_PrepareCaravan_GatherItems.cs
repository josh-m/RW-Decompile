using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_PrepareCaravan_GatherItems : ThinkNode_JobGiver
	{
		private HashSet<Thing> neededItems = new HashSet<Thing>();

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return null;
			}
			Thing thing = this.FindThingToHaul(pawn);
			if (thing == null)
			{
				return null;
			}
			return new Job(JobDefOf.PrepareCaravan_GatherItems, thing);
		}

		private Thing FindThingToHaul(Pawn p)
		{
			this.neededItems.Clear();
			List<TransferableOneWay> transferables = ((LordJob_FormAndSendCaravan)p.GetLord().LordJob).transferables;
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (GatherItemsForCaravanUtility.CountLeftToTransfer(p, transferableOneWay) > 0)
				{
					for (int j = 0; j < transferableOneWay.things.Count; j++)
					{
						this.neededItems.Add(transferableOneWay.things[j]);
					}
				}
			}
			if (!this.neededItems.Any<Thing>())
			{
				return null;
			}
			Predicate<Thing> validator = (Thing x) => this.neededItems.Contains(x) && p.CanReserve(x, 1);
			Thing result = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
			this.neededItems.Clear();
			return result;
		}
	}
}
