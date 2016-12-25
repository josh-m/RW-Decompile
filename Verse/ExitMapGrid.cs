using RimWorld.Planet;
using System;
using UnityEngine;

namespace Verse
{
	public sealed class ExitMapGrid : ICellBoolGiver
	{
		private const int MaxDistToEdge = 4;

		private Map map;

		private bool dirty = true;

		private BoolGrid exitMapGrid;

		private CellBoolDrawer drawerInt;

		public bool MapUsesExitGrid
		{
			get
			{
				if (this.map.IsPlayerHome)
				{
					return false;
				}
				CaravansBattlefield caravansBattlefield = this.map.info.parent as CaravansBattlefield;
				return caravansBattlefield == null || !caravansBattlefield.def.blockExitGridUntilBattleIsWon || caravansBattlefield.WonBattle;
			}
		}

		public CellBoolDrawer Drawer
		{
			get
			{
				if (!this.MapUsesExitGrid)
				{
					return null;
				}
				if (this.dirty)
				{
					this.Rebuild();
				}
				if (this.drawerInt == null)
				{
					this.drawerInt = new CellBoolDrawer(this, this.map.Size.x, this.map.Size.z);
				}
				return this.drawerInt;
			}
		}

		public BoolGrid Grid
		{
			get
			{
				if (!this.MapUsesExitGrid)
				{
					return null;
				}
				if (this.dirty)
				{
					this.Rebuild();
				}
				return this.exitMapGrid;
			}
		}

		public Color Color
		{
			get
			{
				return new Color(0.35f, 1f, 0.35f, 0.18f);
			}
		}

		public ExitMapGrid(Map map)
		{
			this.map = map;
		}

		public bool GetCellBool(int index)
		{
			return this.Grid[index] && !this.map.fogGrid.IsFogged(index);
		}

		public bool IsExitCell(IntVec3 c)
		{
			return this.MapUsesExitGrid && this.Grid[c];
		}

		public void ExitMapGridUpdate()
		{
			if (!this.MapUsesExitGrid)
			{
				return;
			}
			this.Drawer.MarkForDraw();
			this.Drawer.CellBoolDrawerUpdate();
		}

		public void Notify_LOSBlockerSpawned()
		{
			this.dirty = true;
		}

		public void Notify_LOSBlockerDespawned()
		{
			this.dirty = true;
		}

		private void Rebuild()
		{
			this.dirty = false;
			if (this.exitMapGrid == null)
			{
				this.exitMapGrid = new BoolGrid(this.map);
			}
			else
			{
				this.exitMapGrid.Clear();
			}
			CellRect cellRect = CellRect.WholeMap(this.map);
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					if (i > 3 && i < cellRect.maxZ - 4 + 1 && j > 3 && j < cellRect.maxX - 4 + 1)
					{
						j = cellRect.maxX - 4 + 1;
					}
					IntVec3 intVec = new IntVec3(j, 0, i);
					if (this.IsGoodExitCell(intVec))
					{
						this.exitMapGrid[intVec] = true;
					}
				}
			}
			if (this.drawerInt != null)
			{
				this.drawerInt.SetDirty();
			}
		}

		private bool IsGoodExitCell(IntVec3 cell)
		{
			if (!cell.CanBeSeenOver(this.map))
			{
				return false;
			}
			int num = GenRadial.NumCellsInRadius(4f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = cell + GenRadial.RadialPattern[i];
				if (intVec.InBounds(this.map) && intVec.OnEdge(this.map) && intVec.CanBeSeenOverFast(this.map) && GenSight.LineOfSight(cell, intVec, this.map, false))
				{
					return true;
				}
			}
			return false;
		}
	}
}
