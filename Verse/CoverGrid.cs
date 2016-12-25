using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class CoverGrid
	{
		private Map map;

		public Thing[] innerArray;

		public Thing[] InnerArray
		{
			get
			{
				return this.innerArray;
			}
		}

		public Thing this[int index]
		{
			get
			{
				return this.innerArray[index];
			}
		}

		public Thing this[IntVec3 c]
		{
			get
			{
				return this.innerArray[this.map.cellIndices.CellToIndex(c)];
			}
		}

		public CoverGrid(Map map)
		{
			this.map = map;
			this.innerArray = new Thing[map.cellIndices.NumGridCells];
		}

		public void Register(Thing t)
		{
			if (t.def.Fillage == FillCategory.None)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					this.RecalculateCell(c, null);
				}
			}
		}

		public void DeRegister(Thing t)
		{
			if (t.def.Fillage == FillCategory.None)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					this.RecalculateCell(c, t);
				}
			}
		}

		private void RecalculateCell(IntVec3 c, Thing ignoreThing = null)
		{
			Thing thing = null;
			float num = 0.001f;
			List<Thing> list = this.map.thingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing2 != ignoreThing && !thing2.Destroyed && thing2.Spawned)
				{
					if (thing2.def.fillPercent > num)
					{
						thing = thing2;
						num = thing2.def.fillPercent;
					}
				}
			}
			this.innerArray[this.map.cellIndices.CellToIndex(c)] = thing;
		}
	}
}
