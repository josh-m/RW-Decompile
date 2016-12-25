using System;

namespace Verse.AI
{
	public static class ReservationUtility
	{
		public static bool CanReserve(this Pawn p, TargetInfo target, int maxPawns = 1)
		{
			return Find.Reservations.CanReserve(p, target, maxPawns);
		}

		public static bool CanReserveAndReach(this Pawn p, TargetInfo target, PathEndMode peMode, Danger maxDanger, int maxPawns = 1)
		{
			return p.CanReach(target, peMode, maxDanger, false, TraverseMode.ByPawn) && Find.Reservations.CanReserve(p, target, maxPawns);
		}

		public static void Reserve(this Pawn p, TargetInfo target, int maxPawns = 1)
		{
			Find.Reservations.Reserve(p, target, maxPawns);
		}
	}
}
