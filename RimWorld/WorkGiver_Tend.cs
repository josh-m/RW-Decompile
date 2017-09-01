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

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			return pawn2 != null && (!this.def.tendToHumanlikesOnly || pawn2.RaceProps.Humanlike) && (!this.def.tendToAnimalsOnly || pawn2.RaceProps.Animal) && WorkGiver_Tend.GoodLayingStatusForTend(pawn2, pawn) && HealthAIUtility.ShouldBeTendedNow(pawn2) && pawn.CanReserve(pawn2, 1, -1, null, forced);
		}

		public static bool GoodLayingStatusForTend(Pawn patient, Pawn doctor)
		{
			if (patient == doctor)
			{
				return patient.GetPosture() == PawnPosture.Standing;
			}
			if (patient.RaceProps.Humanlike)
			{
				return patient.InBed();
			}
			return patient.GetPosture() != PawnPosture.Standing;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
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
