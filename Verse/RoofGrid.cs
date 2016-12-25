using System;
using UnityEngine;

namespace Verse
{
	public sealed class RoofGrid : ICellBoolGiver, IExposable
	{
		private Map map;

		private ushort[] roofGrid;

		private CellBoolDrawer drawerInt;

		public CellBoolDrawer Drawer
		{
			get
			{
				if (this.drawerInt == null)
				{
					this.drawerInt = new CellBoolDrawer(this, this.map.Size.x, this.map.Size.z);
				}
				return this.drawerInt;
			}
		}

		public Color Color
		{
			get
			{
				return new Color(0.3f, 1f, 0.4f);
			}
		}

		public RoofGrid(Map map)
		{
			this.map = map;
			this.roofGrid = new ushort[map.cellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			string compressedString = string.Empty;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				compressedString = GridSaveUtility.CompressedStringForShortGrid((IntVec3 c) => this.roofGrid[this.map.cellIndices.CellToIndex(c)], this.map);
			}
			Scribe_Values.LookValue<string>(ref compressedString, "roofs", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (GridSaveUtility.LoadedGridShort current in GridSaveUtility.LoadedUShortGrid(compressedString, this.map))
				{
					this.SetRoof(current.cell, DefDatabase<RoofDef>.GetByShortHash(current.val));
				}
			}
		}

		public bool GetCellBool(int index)
		{
			return this.roofGrid[index] != 0 && !this.map.fogGrid.IsFogged(index);
		}

		public bool Roofed(int x, int z)
		{
			return this.roofGrid[this.map.cellIndices.CellToIndex(x, z)] != 0;
		}

		public bool Roofed(IntVec3 c)
		{
			return this.roofGrid[this.map.cellIndices.CellToIndex(c)] != 0;
		}

		public RoofDef RoofAt(IntVec3 c)
		{
			return DefDatabase<RoofDef>.GetByShortHash(this.roofGrid[this.map.cellIndices.CellToIndex(c)]);
		}

		public RoofDef RoofAt(int x, int z)
		{
			return DefDatabase<RoofDef>.GetByShortHash(this.roofGrid[this.map.cellIndices.CellToIndex(x, z)]);
		}

		public void SetRoof(IntVec3 c, RoofDef def)
		{
			ushort num;
			if (def == null)
			{
				num = 0;
			}
			else
			{
				num = def.shortHash;
			}
			if (this.roofGrid[this.map.cellIndices.CellToIndex(c)] == num)
			{
				return;
			}
			this.roofGrid[this.map.cellIndices.CellToIndex(c)] = num;
			this.map.glowGrid.MarkGlowGridDirty(c);
			Room room = c.GetRoom(this.map);
			if (room != null)
			{
				room.RoofChanged();
			}
			if (this.drawerInt != null)
			{
				this.drawerInt.SetDirty();
			}
			this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Roofs);
		}

		public void RoofGridUpdate()
		{
			if (Find.PlaySettings.showRoofOverlay)
			{
				this.Drawer.MarkForDraw();
			}
			this.Drawer.CellBoolDrawerUpdate();
		}
	}
}
