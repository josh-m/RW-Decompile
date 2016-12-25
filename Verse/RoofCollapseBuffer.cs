using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RoofCollapseBuffer
	{
		private static List<IntVec3> cellsToCollapse = new List<IntVec3>();

		private static List<Thing> crushedThingsToReport = new List<Thing>();

		public static List<IntVec3> CellsMarkedToCollapse
		{
			get
			{
				return RoofCollapseBuffer.cellsToCollapse;
			}
		}

		public static List<Thing> CrushedThingsForLetter
		{
			get
			{
				return RoofCollapseBuffer.crushedThingsToReport;
			}
		}

		public static bool IsMarkedToCollapse(IntVec3 c)
		{
			return RoofCollapseBuffer.cellsToCollapse.Contains(c);
		}

		public static void MarkToCollapse(IntVec3 c)
		{
			if (!RoofCollapseBuffer.cellsToCollapse.Contains(c))
			{
				RoofCollapseBuffer.cellsToCollapse.Add(c);
			}
		}

		public static void Notify_Crushed(Thing t)
		{
			if (!RoofCollapseBuffer.crushedThingsToReport.Contains(t) && RoofCollapseBuffer.WorthMentioningInCrushLetter(t))
			{
				RoofCollapseBuffer.crushedThingsToReport.Add(t);
			}
		}

		private static bool WorthMentioningInCrushLetter(Thing t)
		{
			if (!t.def.destroyable)
			{
				return false;
			}
			switch (t.def.category)
			{
			case ThingCategory.Pawn:
				return true;
			case ThingCategory.Item:
				return t.MarketValue > 0.01f;
			case ThingCategory.Building:
				return true;
			}
			return false;
		}

		public static void Clear()
		{
			RoofCollapseBuffer.cellsToCollapse.Clear();
			RoofCollapseBuffer.crushedThingsToReport.Clear();
		}
	}
}
