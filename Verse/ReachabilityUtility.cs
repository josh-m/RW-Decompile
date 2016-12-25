using System;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class ReachabilityUtility
	{
		public static bool CanReach(this Pawn pawn, LocalTargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBash = false, TraverseMode mode = TraverseMode.ByPawn)
		{
			return pawn.Spawned && pawn.Map.reachability.CanReach(pawn.Position, dest, peMode, TraverseParms.For(pawn, maxDanger, mode, canBash));
		}

		public static bool CanReachNonLocal(this Pawn pawn, TargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBash = false, TraverseMode mode = TraverseMode.ByPawn)
		{
			return pawn.Spawned && pawn.Map.reachability.CanReachNonLocal(pawn.Position, dest, peMode, TraverseParms.For(pawn, maxDanger, mode, canBash));
		}

		public static bool CanReachMapEdge(this Pawn p)
		{
			return p.Spawned && p.Map.reachability.CanReachMapEdge(p.Position, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false));
		}

		public static void ClearCache()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].reachability.ClearCache();
			}
		}
	}
}
