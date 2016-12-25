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
				return PathEndMode.OnCell;
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
			return pawn2 != null && pawn2 != pawn && pawn2.InBed() && pawn2.health.ShouldBeTendedNow && pawn.CanReserve(pawn2, 1);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			Thing thing = null;
			if (Medicine.GetMedicineCountToFullyHeal(pawn2) > 0)
			{
				thing = HealWorkGiverUtility.FindBestMedicine(pawn, pawn2);
			}
			if (thing != null)
			{
				return new Job(JobDefOf.TendPatient, pawn2, thing);
			}
			return new Job(JobDefOf.TendPatient, pawn2);
		}
	}
}
