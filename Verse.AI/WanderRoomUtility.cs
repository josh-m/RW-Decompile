using System;

namespace Verse.AI
{
	public static class WanderRoomUtility
	{
		public static bool IsValidWanderDest(Pawn pawn, IntVec3 loc, IntVec3 root)
		{
			Room room = root.GetRoom(pawn.Map, RegionType.Set_Passable);
			return room == null || room.RegionType == RegionType.Portal || WanderUtility.InSameRoom(root, loc, pawn.Map);
		}
	}
}
