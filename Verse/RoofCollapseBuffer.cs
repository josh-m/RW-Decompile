using System;
using System.Collections.Generic;

namespace Verse
{
	public class RoofCollapseBuffer
	{
		private List<IntVec3> cellsToCollapse = new List<IntVec3>();

		public List<IntVec3> CellsMarkedToCollapse
		{
			get
			{
				return this.cellsToCollapse;
			}
		}

		public bool IsMarkedToCollapse(IntVec3 c)
		{
			return this.cellsToCollapse.Contains(c);
		}

		public void MarkToCollapse(IntVec3 c)
		{
			if (!this.cellsToCollapse.Contains(c))
			{
				this.cellsToCollapse.Add(c);
			}
		}

		public void Clear()
		{
			this.cellsToCollapse.Clear();
		}
	}
}
