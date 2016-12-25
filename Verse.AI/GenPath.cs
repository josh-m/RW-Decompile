using System;

namespace Verse.AI
{
	public static class GenPath
	{
		public static void ResolvePathMode(ref TargetInfo dest, ref PathEndMode peMode)
		{
			if (peMode == PathEndMode.InteractionCell)
			{
				dest = dest.Thing.InteractionCell;
				peMode = PathEndMode.OnCell;
				return;
			}
			if (peMode == PathEndMode.ClosestTouch)
			{
				if (Find.PathGrid.PerceivedPathCostAt(dest.Cell) > 30 || !dest.Cell.Walkable())
				{
					peMode = PathEndMode.Touch;
				}
				else
				{
					peMode = PathEndMode.OnCell;
				}
			}
		}
	}
}
