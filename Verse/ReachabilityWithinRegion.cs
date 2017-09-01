using System;
using Verse.AI;

namespace Verse
{
	public static class ReachabilityWithinRegion
	{
		public static bool ThingFromRegionListerReachable(Thing thing, Region region, PathEndMode peMode, Pawn traveler)
		{
			Map map = region.Map;
			if (peMode == PathEndMode.ClosestTouch)
			{
				peMode = GenPath.ResolveClosestTouchPathMode(traveler, map, thing.Position);
			}
			switch (peMode)
			{
			case PathEndMode.None:
				return false;
			case PathEndMode.OnCell:
				if (thing.def.size.x == 1 && thing.def.size.z == 1)
				{
					if (thing.Position.GetRegion(map, RegionType.Set_Passable) == region)
					{
						return true;
					}
				}
				else
				{
					CellRect.CellRectIterator iterator = thing.OccupiedRect().GetIterator();
					while (!iterator.Done())
					{
						if (iterator.Current.GetRegion(map, RegionType.Set_Passable) == region)
						{
							return true;
						}
						iterator.MoveNext();
					}
				}
				return false;
			case PathEndMode.Touch:
				return true;
			case PathEndMode.InteractionCell:
				return thing.InteractionCell.GetRegion(map, RegionType.Set_Passable) == region;
			}
			Log.Error("Unsupported PathEndMode: " + peMode);
			return false;
		}
	}
}
