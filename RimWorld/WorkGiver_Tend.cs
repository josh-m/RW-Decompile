using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Tend : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.InteractionCell;
			}
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			return pawn2 != null && pawn2 != pawn && WorkGiver_Tend.GoodLayingStatusForTend(pawn2) && HealthAIUtility.ShouldBeTendedNow(pawn2) && pawn.CanReserve(pawn2, 1) && pawn.CanReach(t, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn);
		}

		public static bool GoodLayingStatusForTend(Pawn patient)
		{
			if (patient.RaceProps.Humanlike)
			{
				return patient.InBed();
			}
			return patient.GetPosture() != PawnPosture.Standing;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			Thing thing = null;
			if (Medicine.GetMedicineCountToFullyHeal(pawn2) > 0)
			{
				thing = HealthAIUtility.FindBestMedicine(pawn, pawn2);
			}
			if (thing != null)
			{
				return new Job(JobDefOf.TendPatient, pawn2, thing);
			}
			return new Job(JobDefOf.TendPatient, pawn2);
		}
	}
}
