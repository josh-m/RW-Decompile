using System;
using Verse;

namespace RimWorld
{
	public static class InventoryCalculatorsUtility
	{
		public static bool ShouldIgnoreInventoryOf(Pawn pawn, IgnorePawnsInventoryMode ignoreMode)
		{
			switch (ignoreMode)
			{
			case IgnorePawnsInventoryMode.Ignore:
				return true;
			case IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload:
				return pawn.Spawned && pawn.inventory.UnloadEverything;
			case IgnorePawnsInventoryMode.DontIgnore:
				return false;
			default:
				throw new NotImplementedException("IgnorePawnsInventoryMode");
			}
		}
	}
}
