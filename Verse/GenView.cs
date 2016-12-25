using System;
using UnityEngine;

namespace Verse
{
	public static class GenView
	{
		private const int ViewRectMargin = 5;

		private static CellRect viewRect;

		public static bool ShouldSpawnMotesAt(this Vector3 loc)
		{
			return loc.ToIntVec3().ShouldSpawnMotesAt();
		}

		public static bool ShouldSpawnMotesAt(this IntVec3 loc)
		{
			if (!loc.InBounds())
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
			GenView.viewRect.ClipInsideMap();
			return GenView.viewRect.RandomVector3;
		}
	}
}
