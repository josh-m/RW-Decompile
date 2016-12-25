using System;

namespace Verse.AI
{
	public static class ReservationUtility
	{
		public static bool CanReserve(this Pawn p, LocalTargetInfo target, int maxPawns = 1)
		{
			return p.Spawned && p.Map.reservationManager.CanReserve(p, target, maxPawns);
		}

		public static bool CanReserveAndReach(this Pawn p, LocalTargetInfo target, PathEndMode peMode, Danger maxDanger, int maxPawns = 1)
		{
			return p.Spawned && p.CanReach(target, peMode, maxDanger, false, TraverseMode.ByPawn) && p.Map.reservationManager.CanReserve(p, target, maxPawns);
		}

		public static bool Reserve(this Pawn p, LocalTargetInfo target, int maxPawns = 1)
		{
			return p.Spawned && p.Map.reservationManager.Reserve(p, target, maxPawns);
		}
	}
}
