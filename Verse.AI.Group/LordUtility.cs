using System;

namespace Verse.AI.Group
{
	public static class LordUtility
	{
		public static Lord GetLord(this Pawn p)
		{
			return Find.LordManager.LordOf(p);
		}
	}
}
