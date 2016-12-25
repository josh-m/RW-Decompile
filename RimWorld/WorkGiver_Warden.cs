using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Warden : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		protected bool ShouldTakeCareOfPrisoner(Pawn warden, Thing prisoner)
		{
			Pawn pawn = prisoner as Pawn;
			return pawn != null && pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure && pawn.holder == null && !pawn.InAggroMentalState && !prisoner.IsForbidden(warden) && warden.CanReserveAndReach(pawn, PathEndMode.OnCell, warden.NormalMaxDanger(), 1);
		}
	}
}
