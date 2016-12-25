using System;
using System.Collections.Generic;

namespace Verse
{
	public class LinkGrid
	{
		private Map map;

		private LinkFlags[] linkGrid;

		public LinkGrid(Map map)
		{
			this.map = map;
			this.linkGrid = new LinkFlags[map.cellIndices.NumGridCells];
		}

		public LinkFlags LinkFlagsAt(IntVec3 c)
		{
			return this.linkGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public void Notify_LinkerCreatedOrDestroyed(Thing linker)
		{
			CellIndices cellIndices = this.map.cellIndices;
			CellRect.CellRectIterator iterator = linker.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				LinkFlags linkFlags = LinkFlags.None;
				List<Thing> list = this.map.thingGrid.ThingsListAt(current);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def.graphicData != null)
					{
						linkFlags |= list[i].def.graphicData.linkFlags;
					}
				}
				this.linkGrid[cellIndices.CellToIndex(current)] = linkFlags;
				iterator.MoveNext();
			}
		}
	}
}
