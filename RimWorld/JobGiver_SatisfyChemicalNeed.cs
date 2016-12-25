using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SatisfyChemicalNeed : ThinkNode_JobGiver
	{
		private static List<Need_Chemical> tmpChemicalNeeds = new List<Need_Chemical>();

		public override float GetPriority(Pawn pawn)
		{
			if (pawn.needs.AllNeeds.Any((Need x) => this.ShouldSatisfy(x)))
			{
				return 9.25f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.Clear();
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				if (this.ShouldSatisfy(allNeeds[i]))
				{
					JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.Add((Need_Chemical)allNeeds[i]);
				}
			}
			if (!JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.Any<Need_Chemical>())
			{
				return null;
			}
			JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.SortBy((Need_Chemical x) => x.CurLevel);
			for (int j = 0; j < JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.Count; j++)
			{
				Thing thing = this.FindDrugFor(pawn, JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds[j]);
				if (thing != null)
				{
					JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.Clear();
					return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
				}
			}
			JobGiver_SatisfyChemicalNeed.tmpChemicalNeeds.Clear();
			return null;
		}

		private bool ShouldSatisfy(Need need)
		{
			Need_Chemical need_Chemical = need as Need_Chemical;
			return need_Chemical != null && need_Chemical.CurCategory <= DrugDesireCategory.Desire;
		}

		private Thing FindDrugFor(Pawn pawn, Need_Chemical need)
		{
			Hediff_Addiction addictionHediff = need.AddictionHediff;
			if (addictionHediff == null)
			{
				return null;
			}
			ThingContainer innerContainer = pawn.inventory.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (this.DrugValidator(pawn, addictionHediff, innerContainer[i]))
				{
					return innerContainer[i];
				}
			}
			Predicate<Thing> validator = (Thing x) => this.DrugValidator(pawn, addictionHediff, x);
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}

		private bool DrugValidator(Pawn pawn, Hediff_Addiction addiction, Thing drug)
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
			CompDrug compDrug = drug.TryGetComp<CompDrug>();
			if (compDrug == null || compDrug.Props.chemical == null)
			{
				return false;
			}
			if (compDrug.Props.chemical.addictionHediff != addiction.def)
			{
				return false;
			}
			if (pawn.drugs != null && !pawn.drugs.CurrentPolicy[drug.def].allowedForAddiction && pawn.story != null)
			{
				int num = pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
				if (num <= 0 && !pawn.InMentalState)
				{
					return false;
				}
			}
			return true;
		}
	}
}
