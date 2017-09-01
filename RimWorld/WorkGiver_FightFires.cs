using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal class WorkGiver_FightFires : WorkGiver_Scanner
	{
		private const int NearbyPawnRadius = 15;

		private const int MaxReservationCheckDistance = 15;

		private const float HandledDistance = 5f;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.Fire);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Fire fire = t as Fire;
			if (fire == null)
			{
				return false;
			}
			Pawn pawn2 = fire.parent as Pawn;
			if (pawn2 != null)
			{
				if (pawn2 == pawn)
				{
					return false;
				}
				if ((pawn2.Faction == pawn.Faction || pawn2.HostFaction == pawn.Faction || pawn2.HostFaction == pawn.HostFaction) && !pawn.Map.areaManager.Home[fire.Position] && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, pawn2.Position) > 15)
				{
					return false;
				}
				if (!pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return false;
				}
			}
			else
			{
				if (pawn.story.WorkTagIsDisabled(WorkTags.Firefighting))
				{
					return false;
				}
				if (!pawn.Map.areaManager.Home[fire.Position])
				{
					JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
					return false;
				}
				if (!pawn.CanReach(fire.Position, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					return false;
				}
			}
			return ((pawn.Position - fire.Position).LengthHorizontalSquared <= 225 || pawn.CanReserve(fire, 1, -1, null, forced)) && !WorkGiver_FightFires.FireIsBeingHandled(fire, pawn);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.BeatFire, t);
		}

		public static bool FireIsBeingHandled(Fire f, Pawn potentialHandler)
		{
			if (!f.Spawned)
			{
				return false;
			}
			Pawn pawn = f.Map.reservationManager.FirstReserverWhoseReservationsRespects(f, potentialHandler);
			return pawn != null && pawn.Position.InHorDistOf(f.Position, 5f);
		}
	}
}
