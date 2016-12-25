using System;

namespace Verse.AI
{
	public static class GenPath
	{
		public static TargetInfo ResolvePathMode(TargetInfo dest, ref PathEndMode peMode)
		{
			if (peMode == PathEndMode.InteractionCell)
			{
				peMode = PathEndMode.OnCell;
				return new TargetInfo(dest.Thing.InteractionCell, dest.Thing.Map, false);
			}
			if (peMode == PathEndMode.ClosestTouch)
			{
				if (dest.Map.pathGrid.PerceivedPathCostAt(dest.Cell) > 30 || !dest.Cell.Walkable(dest.Map))
				{
					peMode = PathEndMode.Touch;
				}
				else
				{
					peMode = PathEndMode.OnCell;
				}
			}
			return dest;
		}
	}
}
