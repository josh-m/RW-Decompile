using System;
using System.Collections.Generic;

namespace Verse
{
	public class BoolGrid : IExposable
	{
		private bool[] arr;

		private int trueCountInt;

		private int mapSizeX;

		private int mapSizeZ;

		public int TrueCount
		{
			get
			{
				return this.trueCountInt;
			}
		}

		public IEnumerable<IntVec3> ActiveCells
		{
			get
			{
				if (this.trueCountInt != 0)
				{
					int yieldedCount = 0;
					for (int i = 0; i < this.arr.Length; i++)
					{
						if (this.arr[i])
						{
							yield return CellIndicesUtility.IndexToCell(i, this.mapSizeX);
							yieldedCount++;
							if (yieldedCount >= this.trueCountInt)
							{
								break;
							}
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
				return this.arr[CellIndicesUtility.CellToIndex(c, this.mapSizeX)];
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
				return this.arr[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)];
			}
			set
			{
				this.Set(CellIndicesUtility.CellToIndex(x, z, this.mapSizeX), value);
			}
		}

		public BoolGrid()
		{
		}

		public BoolGrid(Map map)
		{
			this.ClearAndResizeTo(map);
		}

		public bool MapSizeMatches(Map map)
		{
			return this.mapSizeX == map.Size.x && this.mapSizeZ == map.Size.z;
		}

		public void ClearAndResizeTo(Map map)
		{
			if (this.MapSizeMatches(map) && this.arr != null)
			{
				this.Clear();
				return;
			}
			this.mapSizeX = map.Size.x;
			this.mapSizeZ = map.Size.z;
			this.arr = new bool[this.mapSizeX * this.mapSizeZ];
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.trueCountInt, "trueCount", 0, false);
			Scribe_Values.Look<int>(ref this.mapSizeX, "mapSizeX", 0, false);
			Scribe_Values.Look<int>(ref this.mapSizeZ, "mapSizeZ", 0, false);
			ArrayExposeUtility.ExposeBoolArray(ref this.arr, this.mapSizeX, this.mapSizeZ, "arr");
		}

		public void Clear()
		{
			Array.Clear(this.arr, 0, this.arr.Length);
			this.trueCountInt = 0;
		}

		public virtual void Set(IntVec3 c, bool value)
		{
			this.Set(CellIndicesUtility.CellToIndex(c, this.mapSizeX), value);
		}

		public virtual void Set(int index, bool value)
		{
			if (this.arr[index] == value)
			{
				return;
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
			}
			this.trueCountInt = this.arr.Length - this.trueCountInt;
		}
	}
}
