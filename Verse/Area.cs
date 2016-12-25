using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class Area : ICellBoolGiver, ILoadReferenceable, IExposable
	{
		public AreaManager areaManager;

		public int ID = -1;

		private BoolGrid innerGrid;

		private CellBoolDrawer drawer;

		private Texture2D colorTextureInt;

		public Map Map
		{
			get
			{
				return this.areaManager.map;
			}
		}

		public int TrueCount
		{
			get
			{
				return this.innerGrid.TrueCount;
			}
		}

		public abstract string Label
		{
			get;
		}

		public abstract Color Color
		{
			get;
		}

		public abstract int ListPriority
		{
			get;
		}

		public Texture2D ColorTexture
		{
			get
			{
				if (this.colorTextureInt == null)
				{
					this.colorTextureInt = SolidColorMaterials.NewSolidColorTexture(this.Color);
				}
				return this.colorTextureInt;
			}
		}

		public bool this[int index]
		{
			get
			{
				return this.innerGrid[index];
			}
			set
			{
				this.Set(this.Map.cellIndices.IndexToCell(index), value);
			}
		}

		public bool this[IntVec3 c]
		{
			get
			{
				return this.innerGrid[this.Map.cellIndices.CellToIndex(c)];
			}
			set
			{
				this.Set(c, value);
			}
		}

		private CellBoolDrawer Drawer
		{
			get
			{
				if (this.drawer == null)
				{
					this.drawer = new CellBoolDrawer(this, this.Map.Size.x, this.Map.Size.z);
				}
				return this.drawer;
			}
		}

		public IEnumerable<IntVec3> ActiveCells
		{
			get
			{
				return this.innerGrid.ActiveCells;
			}
		}

		public virtual bool Mutable
		{
			get
			{
				return false;
			}
		}

		public Area()
		{
		}

		public Area(AreaManager areaManager)
		{
			this.areaManager = areaManager;
			this.innerGrid = new BoolGrid(areaManager.map);
			this.ID = Find.UniqueIDsManager.GetNextAreaID();
		}

		public virtual void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ID, "ID", -1, false);
			Scribe_Deep.LookDeep<BoolGrid>(ref this.innerGrid, "innerGrid", new object[0]);
		}

		public bool GetCellBool(int index)
		{
			return this.innerGrid[index];
		}

		public virtual bool AssignableAsAllowed(AllowedAreaMode mode)
		{
			return false;
		}

		public virtual void SetLabel(string label)
		{
			throw new NotImplementedException();
		}

		protected virtual void Set(IntVec3 c, bool val)
		{
			int index = this.Map.cellIndices.CellToIndex(c);
			if (this.innerGrid[index] == val)
			{
				return;
			}
			this.innerGrid[index] = val;
			this.MarkDirty(c);
		}

		private void MarkDirty(IntVec3 c)
		{
			this.Drawer.SetDirty();
			Region region = c.GetRegion(this.Map);
			if (region != null)
			{
				region.Notify_AreaChanged(this);
			}
		}

		public void Delete()
		{
			this.areaManager.Remove(this);
		}

		public void MarkForDraw()
		{
			if (this.Map == Find.VisibleMap)
			{
				this.Drawer.MarkForDraw();
			}
		}

		public void AreaUpdate()
		{
			this.Drawer.CellBoolDrawerUpdate();
		}

		public void Invert()
		{
			this.innerGrid.Invert();
			this.Drawer.SetDirty();
		}

		public abstract string GetUniqueLoadID();
	}
}
