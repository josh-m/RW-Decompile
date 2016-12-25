using System;
using System.Collections.Generic;

namespace Verse
{
	public class RoofCollapseBuffer
	{
		private List<IntVec3> cellsToCollapse = new List<IntVec3>();

		private List<Thing> crushedThingsToReport = new List<Thing>();

		public List<IntVec3> CellsMarkedToCollapse
		{
			get
			{
				return this.cellsToCollapse;
			}
		}

		public List<Thing> CrushedThingsForLetter
		{
			get
			{
				return this.crushedThingsToReport;
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

		public void Notify_Crushed(Thing t)
		{
			if (!this.crushedThingsToReport.Contains(t) && this.WorthMentioningInCrushLetter(t))
			{
				this.crushedThingsToReport.Add(t);
			}
		}

		private bool WorthMentioningInCrushLetter(Thing t)
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

		public void Clear()
		{
			this.cellsToCollapse.Clear();
			this.crushedThingsToReport.Clear();
		}
	}
}
