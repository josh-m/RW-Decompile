using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public sealed class RoofGrid : IExposable, ICellBoolGiver
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
					this.drawerInt = new CellBoolDrawer(this, this.map.Size.x, this.map.Size.z, 0.33f);
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
			MapExposeUtility.ExposeUshort(this.map, (IntVec3 c) => this.roofGrid[this.map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, ushort val)
			{
				this.SetRoof(c, DefDatabase<RoofDef>.GetByShortHash(val));
			}, "roofs");
		}

		public bool GetCellBool(int index)
		{
			return this.roofGrid[index] != 0 && !this.map.fogGrid.IsFogged(index);
		}

		public Color GetCellExtraColor(int index)
		{
			if (RoofDefOf.RoofRockThick != null && this.roofGrid[index] == RoofDefOf.RoofRockThick.shortHash)
			{
				return Color.gray;
			}
			return Color.white;
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
			Region validRegionAt_NoRebuild = this.map.regionGrid.GetValidRegionAt_NoRebuild(c);
			if (validRegionAt_NoRebuild != null)
			{
				validRegionAt_NoRebuild.Room.Notify_RoofChanged();
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
