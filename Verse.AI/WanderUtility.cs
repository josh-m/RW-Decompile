using System;

namespace Verse.AI
{
	public static class WanderUtility
	{
		public static IntVec3 BestCloseWanderRoot(IntVec3 trueWanderRoot, Pawn pawn)
		{
			for (int i = 0; i < 50; i++)
			{
				IntVec3 intVec;
				if (i < 8)
				{
					intVec = trueWanderRoot + GenRadial.RadialPattern[i];
				}
				else
				{
					intVec = trueWanderRoot + GenRadial.RadialPattern[i - 8 + 1] * 7;
				}
				if (intVec.InBounds(pawn.Map) && intVec.Walkable(pawn.Map) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Some, false, TraverseMode.ByPawn))
				{
					return intVec;
				}
			}
			return IntVec3.Invalid;
		}

		public static bool InSameRoom(IntVec3 locA, IntVec3 locB, Map map)
		{
			Room room = locA.GetRoom(map);
			return room == null || room == locB.GetRoom(map);
		}
	}
}
