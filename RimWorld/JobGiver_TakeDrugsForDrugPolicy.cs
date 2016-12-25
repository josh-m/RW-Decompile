using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_TakeDrugsForDrugPolicy : ThinkNode_JobGiver
	{
		public override float GetPriority(Pawn pawn)
		{
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug))
				{
					return 7.5f;
				}
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug))
				{
					Thing thing = this.FindDrugFor(pawn, currentPolicy[i].drug);
					if (thing != null)
					{
						return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
					}
				}
			}
			return null;
		}

		private Thing FindDrugFor(Pawn pawn, ThingDef drugDef)
		{
			ThingContainer container = pawn.inventory.container;
			for (int i = 0; i < container.Count; i++)
			{
				if (container[i].def == drugDef && this.DrugValidator(pawn, container[i]))
				{
					return container[i];
				}
			}
			Predicate<Thing> validator = (Thing x) => this.DrugValidator(pawn, x);
			return GenClosest.ClosestThingReachable(pawn.Position, ThingRequest.ForDef(drugDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}

		private bool DrugValidator(Pawn pawn, Thing drug)
		{
			if (!drug.def.IsDrug)
			{
				return false;
			}
			if (drug.Spawned)
			{
				if (drug.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(drug, 1))
				{
					return false;
				}
				if (!drug.IsSociallyProper(pawn))
				{
					return false;
				}
			}
			return true;
		}
	}
}
