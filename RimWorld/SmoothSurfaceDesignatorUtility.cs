using System;
using Verse;

namespace RimWorld
{
	public static class SmoothSurfaceDesignatorUtility
	{
		public static bool CanSmoothFloorUnder(Building b)
		{
			return b.def.Fillage != FillCategory.Full || b.def.passability != Traversability.Impassable;
		}

		public static void Notify_BuildingSpawned(Building b)
		{
			if (!SmoothSurfaceDesignatorUtility.CanSmoothFloorUnder(b))
			{
				CellRect.CellRectIterator iterator = b.OccupiedRect().GetIterator();
				while (!iterator.Done())
				{
					Designation designation = b.Map.designationManager.DesignationAt(iterator.Current, DesignationDefOf.SmoothFloor);
					if (designation != null)
					{
						b.Map.designationManager.RemoveDesignation(designation);
					}
					iterator.MoveNext();
				}
			}
		}

		public static void Notify_BuildingDespawned(Building b, Map map)
		{
			CellRect.CellRectIterator iterator = b.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				Designation designation = map.designationManager.DesignationAt(iterator.Current, DesignationDefOf.SmoothWall);
				if (designation != null)
				{
					map.designationManager.RemoveDesignation(designation);
				}
				iterator.MoveNext();
			}
		}
	}
}
