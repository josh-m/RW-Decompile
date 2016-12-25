using System;
using System.Collections.Generic;

namespace Verse
{
	public static class LinkGrid
	{
		private static LinkFlags[] linkGrid;

		public static void Reinit()
		{
			LinkGrid.linkGrid = new LinkFlags[CellIndices.NumGridCells];
		}

		public static LinkFlags LinkFlagsAt(IntVec3 c)
		{
			return LinkGrid.linkGrid[CellIndices.CellToIndex(c)];
		}

		public static void Notify_LinkerCreatedOrDestroyed(Thing linker)
		{
			CellRect.CellRectIterator iterator = linker.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				LinkFlags linkFlags = LinkFlags.None;
				List<Thing> list = Find.ThingGrid.ThingsListAt(current);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def.graphicData != null)
					{
						linkFlags |= list[i].def.graphicData.linkFlags;
					}
				}
				LinkGrid.linkGrid[CellIndices.CellToIndex(current)] = linkFlags;
				iterator.MoveNext();
			}
		}
	}
}
