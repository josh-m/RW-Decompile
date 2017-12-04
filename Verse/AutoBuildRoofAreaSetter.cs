using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class AutoBuildRoofAreaSetter
	{
		private Map map;

		private List<Room> queuedGenerateRooms = new List<Room>();

		private HashSet<IntVec3> cellsToRoof = new HashSet<IntVec3>();

		private HashSet<IntVec3> innerCells = new HashSet<IntVec3>();

		private List<IntVec3> justRoofedCells = new List<IntVec3>();

		public AutoBuildRoofAreaSetter(Map map)
		{
			this.map = map;
		}

		public void TryGenerateAreaOnImpassable(IntVec3 c)
		{
			if (!c.Roofed(this.map) && c.Impassable(this.map) && RoofCollapseUtility.WithinRangeOfRoofHolder(c, this.map))
			{
				bool flag = false;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 loc = c + GenRadial.RadialPattern[i];
					Room room = loc.GetRoom(this.map, RegionType.Set_Passable);
					if (room != null && !room.TouchesMapEdge)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					this.map.areaManager.BuildRoof[c] = true;
					MoteMaker.PlaceTempRoof(c, this.map);
				}
			}
		}

		public void TryGenerateAreaFor(Room room)
		{
			this.queuedGenerateRooms.Add(room);
		}

		public void AutoBuildRoofAreaSetterTick_First()
		{
			this.ResolveQueuedGenerateRoofs();
		}

		public void ResolveQueuedGenerateRoofs()
		{
			for (int i = 0; i < this.queuedGenerateRooms.Count; i++)
			{
				this.TryGenerateAreaNow(this.queuedGenerateRooms[i]);
			}
			this.queuedGenerateRooms.Clear();
		}

		private void TryGenerateAreaNow(Room room)
		{
			if (room.Dereferenced || room.TouchesMapEdge)
			{
				return;
			}
			if (room.RegionCount > 26 || room.CellCount > 320)
			{
				return;
			}
			if (room.RegionType == RegionType.Portal)
			{
				return;
			}
			bool flag = false;
			foreach (IntVec3 current in room.BorderCells)
			{
				Thing roofHolderOrImpassable = current.GetRoofHolderOrImpassable(this.map);
				if (roofHolderOrImpassable != null)
				{
					if (roofHolderOrImpassable.Faction != null && roofHolderOrImpassable.Faction != Faction.OfPlayer)
					{
						return;
					}
					if (roofHolderOrImpassable.def.building != null && !roofHolderOrImpassable.def.building.allowAutoroof)
					{
						return;
					}
					if (roofHolderOrImpassable.Faction == Faction.OfPlayer)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			this.innerCells.Clear();
			foreach (IntVec3 current2 in room.Cells)
			{
				if (!this.innerCells.Contains(current2))
				{
					this.innerCells.Add(current2);
				}
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = current2 + GenAdj.AdjacentCells[i];
					if (c.InBounds(this.map))
					{
						Thing roofHolderOrImpassable2 = c.GetRoofHolderOrImpassable(this.map);
						if (roofHolderOrImpassable2 != null && (roofHolderOrImpassable2.def.size.x > 1 || roofHolderOrImpassable2.def.size.z > 1))
						{
							CellRect cellRect = roofHolderOrImpassable2.OccupiedRect();
							cellRect.ClipInsideMap(this.map);
							for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
							{
								for (int k = cellRect.minX; k <= cellRect.maxX; k++)
								{
									IntVec3 item = new IntVec3(k, 0, j);
									if (!this.innerCells.Contains(item))
									{
										this.innerCells.Add(item);
									}
								}
							}
						}
					}
				}
			}
			this.cellsToRoof.Clear();
			foreach (IntVec3 current3 in this.innerCells)
			{
				for (int l = 0; l < 9; l++)
				{
					IntVec3 intVec = current3 + GenAdj.AdjacentCellsAndInside[l];
					if (intVec.InBounds(this.map) && (l == 8 || intVec.GetRoofHolderOrImpassable(this.map) != null) && !this.cellsToRoof.Contains(intVec))
					{
						this.cellsToRoof.Add(intVec);
					}
				}
			}
			this.justRoofedCells.Clear();
			foreach (IntVec3 current4 in this.cellsToRoof)
			{
				if (this.map.roofGrid.RoofAt(current4) == null && !this.justRoofedCells.Contains(current4))
				{
					if (!this.map.areaManager.NoRoof[current4] && RoofCollapseUtility.WithinRangeOfRoofHolder(current4, this.map))
					{
						this.map.areaManager.BuildRoof[current4] = true;
						this.justRoofedCells.Add(current4);
					}
				}
			}
		}
	}
}
