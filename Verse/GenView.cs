using System;
using UnityEngine;

namespace Verse
{
	public static class GenView
	{
		private const int ViewRectMargin = 5;

		private static CellRect viewRect;

		public static bool ShouldSpawnMotesAt(this Vector3 loc, Map map)
		{
			return loc.ToIntVec3().ShouldSpawnMotesAt(map);
		}

		public static bool ShouldSpawnMotesAt(this IntVec3 loc, Map map)
		{
			if (map != Find.VisibleMap)
			{
				return false;
			}
			if (!loc.InBounds(map))
			{
				return false;
			}
			GenView.viewRect = Find.CameraDriver.CurrentViewRect;
			GenView.viewRect = GenView.viewRect.ExpandedBy(5);
			return GenView.viewRect.Contains(loc);
		}

		public static Vector3 RandomPositionOnOrNearScreen()
		{
			GenView.viewRect = Find.CameraDriver.CurrentViewRect;
			GenView.viewRect = GenView.viewRect.ExpandedBy(5);
			GenView.viewRect.ClipInsideMap(Find.VisibleMap);
			return GenView.viewRect.RandomVector3;
		}
	}
}
