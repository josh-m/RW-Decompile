using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class AutoBuildRoofZoneSetter
	{
		private static List<Room> queuedGenerateRooms = new List<Room>();

		private static HashSet<IntVec3> cellsToRoof = new HashSet<IntVec3>();

		private static HashSet<IntVec3> innerCells = new HashSet<IntVec3>();

		private static List<IntVec3> justRoofedCells = new List<IntVec3>();

		public static void TryGenerateRoofOnImpassable(IntVec3 c)
		{
			if (!c.Roofed() && c.Impassable() && RoofCollapseUtility.WithinRangeOfRoofHolder(c))
			{
				bool flag = false;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 loc = c + GenRadial.RadialPattern[i];
					Room room = loc.GetRoom();
					if (room != null && !room.TouchesMapEdge)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Find.AreaBuildRoof.Set(c);
					MoteMaker.PlaceTempRoof(c);
				}
			}
		}

		public static void TryGenerateRoofFor(Room room)
		{
			AutoBuildRoofZoneSetter.queuedGenerateRooms.Add(room);
		}

		public static void AutoBuildRoofZoneSetterTick_First()
		{
			AutoBuildRoofZoneSetter.ResolveQueuedGenerateRoofs();
		}

		public static void ResolveQueuedGenerateRoofs()
		{
			for (int i = 0; i < AutoBuildRoofZoneSetter.queuedGenerateRooms.Count; i++)
			{
				AutoBuildRoofZoneSetter.TryGenerateRoofNow(AutoBuildRoofZoneSetter.queuedGenerateRooms[i]);
			}
			AutoBuildRoofZoneSetter.queuedGenerateRooms.Clear();
		}

		private static void TryGenerateRoofNow(Room room)
		{
			if (room.Dereferenced || room.TouchesMapEdge)
			{
				return;
			}
			if (room.RegionCount > 26 || room.CellCount > 320)
			{
				return;
			}
			AutoBuildRoofZoneSetter.innerCells.Clear();
			foreach (IntVec3 current in room.Cells)
			{
				if (!AutoBuildRoofZoneSetter.innerCells.Contains(current))
				{
					AutoBuildRoofZoneSetter.innerCells.Add(current);
				}
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = current + GenAdj.AdjacentCells[i];
					if (c.InBounds())
					{
						Thing edifice = c.GetEdifice();
						if (edifice != null && edifice.def.regionBarrier && (edifice.def.size.x > 1 || edifice.def.size.z > 1))
						{
							CellRect cellRect = edifice.OccupiedRect();
							cellRect.ClipInsideMap();
							for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
							{
								for (int k = cellRect.minX; k <= cellRect.maxX; k++)
								{
									IntVec3 item = new IntVec3(k, 0, j);
									if (!AutoBuildRoofZoneSetter.innerCells.Contains(item))
									{
										AutoBuildRoofZoneSetter.innerCells.Add(item);
									}
								}
							}
						}
					}
				}
			}
			AutoBuildRoofZoneSetter.cellsToRoof.Clear();
			foreach (IntVec3 current2 in AutoBuildRoofZoneSetter.innerCells)
			{
				for (int l = 0; l < 9; l++)
				{
					IntVec3 intVec = current2 + GenAdj.AdjacentCellsAndInside[l];
					if (intVec.InBounds() && (l == 8 || intVec.GetRegionBarrier() != null) && !AutoBuildRoofZoneSetter.cellsToRoof.Contains(intVec))
					{
						AutoBuildRoofZoneSetter.cellsToRoof.Add(intVec);
					}
				}
			}
			AutoBuildRoofZoneSetter.justRoofedCells.Clear();
			foreach (IntVec3 current3 in AutoBuildRoofZoneSetter.cellsToRoof)
			{
				if (Find.RoofGrid.RoofAt(current3) == null && !AutoBuildRoofZoneSetter.justRoofedCells.Contains(current3))
				{
					if (!Find.AreaNoRoof[current3] && RoofCollapseUtility.WithinRangeOfRoofHolder(current3))
					{
						Find.AreaBuildRoof.Set(current3);
						AutoBuildRoofZoneSetter.justRoofedCells.Add(current3);
					}
				}
			}
		}
	}
}
