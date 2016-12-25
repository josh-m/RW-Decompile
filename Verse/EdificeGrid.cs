using System;

namespace Verse
{
	public sealed class EdificeGrid
	{
		private Building[] innerArray;

		public Building[] InnerArray
		{
			get
			{
				return this.innerArray;
			}
		}

		public Building this[int index]
		{
			get
			{
				return this.innerArray[index];
			}
		}

		public Building this[IntVec3 c]
		{
			get
			{
				return this.innerArray[CellIndices.CellToIndex(c)];
			}
		}

		public EdificeGrid()
		{
			this.innerArray = new Building[CellIndices.NumGridCells];
		}

		public void Register(Building ed)
		{
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					if (UnityData.isDebugBuild && this[intVec] != null && !this[intVec].Destroyed)
					{
						Log.Error(string.Concat(new object[]
						{
							"Added edifice ",
							ed.LabelCap,
							" over edifice ",
							this[intVec].LabelCap,
							" at ",
							intVec,
							". Destroying old edifice."
						}));
						this[intVec].Destroy(DestroyMode.Vanish);
						return;
					}
					this.innerArray[CellIndices.CellToIndex(intVec)] = ed;
				}
			}
		}

		public void DeRegister(Building ed)
		{
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					this.innerArray[CellIndices.CellToIndex(j, i)] = null;
				}
			}
		}
	}
}
