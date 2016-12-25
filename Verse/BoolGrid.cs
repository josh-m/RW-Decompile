using System;
using System.Collections.Generic;

namespace Verse
{
	public class BoolGrid : IExposable
	{
		private bool[] arr;

		private int trueCountInt;

		private MapMeshFlag mapChangeType;

		public int TrueCount
		{
			get
			{
				return this.trueCountInt;
			}
		}

		public bool[] InnerArray
		{
			get
			{
				return this.arr;
			}
		}

		public IEnumerable<IntVec3> ActiveCells
		{
			get
			{
				if (this.trueCountInt != 0)
				{
					for (int i = 0; i < this.arr.Length; i++)
					{
						if (this.arr[i])
						{
							yield return CellIndices.IndexToCell(i);
						}
					}
				}
			}
		}

		public bool this[int index]
		{
			get
			{
				return this.arr[index];
			}
			set
			{
				this.Set(index, value);
			}
		}

		public bool this[IntVec3 c]
		{
			get
			{
				return this.arr[CellIndices.CellToIndex(c)];
			}
			set
			{
				this.Set(c, value);
			}
		}

		public bool this[int x, int z]
		{
			get
			{
				return this.arr[CellIndices.CellToIndex(x, z)];
			}
			set
			{
				this.Set(CellIndices.CellToIndex(x, z), value);
			}
		}

		public BoolGrid()
		{
			if (this.arr == null)
			{
				this.arr = new bool[CellIndices.NumGridCells];
			}
		}

		public BoolGrid(MapMeshFlag mapChangeType) : this()
		{
			this.mapChangeType = mapChangeType;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.trueCountInt, "trueCount", 0, false);
			ArrayExposeUtility.ExposeBoolArray(ref this.arr, "arr");
		}

		public void Clear()
		{
			int numGridCells = CellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				this.arr[i] = false;
			}
		}

		public virtual void Set(IntVec3 c, bool value)
		{
			this.Set(CellIndices.CellToIndex(c), value);
		}

		public virtual void Set(int index, bool value)
		{
			if (this.arr[index] == value)
			{
				return;
			}
			if (this.mapChangeType != MapMeshFlag.None)
			{
				Find.MapDrawer.MapMeshDirty(CellIndices.IndexToCell(index), this.mapChangeType);
			}
			this.arr[index] = value;
			if (value)
			{
				this.trueCountInt++;
			}
			else
			{
				this.trueCountInt--;
			}
		}

		public void Invert()
		{
			for (int i = 0; i < this.arr.Length; i++)
			{
				this.arr[i] = !this.arr[i];
				if (this.mapChangeType != MapMeshFlag.None)
				{
					Find.MapDrawer.MapMeshDirty(CellIndices.IndexToCell(i), this.mapChangeType);
				}
			}
			this.trueCountInt = this.arr.Length - this.trueCountInt;
		}
	}
}
