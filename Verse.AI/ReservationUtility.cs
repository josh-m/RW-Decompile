using System;

namespace Verse.AI
{
	public static class ReservationUtility
	{
		public static bool CanReserve(this Pawn p, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
		{
			return p.Spawned && p.Map.reservationManager.CanReserve(p, target, maxPawns, stackCount, layer, ignoreOtherReservations);
		}

		public static bool CanReserveAndReach(this Pawn p, LocalTargetInfo target, PathEndMode peMode, Danger maxDanger, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
		{
			return p.Spawned && p.CanReach(target, peMode, maxDanger, false, TraverseMode.ByPawn) && p.Map.reservationManager.CanReserve(p, target, maxPawns, stackCount, layer, ignoreOtherReservations);
		}

		public static bool Reserve(this Pawn p, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null)
		{
			return p.Spawned && p.Map.reservationManager.Reserve(p, target, maxPawns, stackCount, layer);
		}

		public static bool HasReserved(this Pawn p, LocalTargetInfo target)
		{
			return p.Spawned && p.Map.reservationManager.ReservedBy(target, p);
		}
	}
}
