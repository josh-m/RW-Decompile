using System;
using Verse;

namespace RimWorld
{
	public static class MiscDebugDrawer
	{
		public static void DebugDrawInteractionCells()
		{
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (DebugViewSettings.drawInteractionCells)
			{
				foreach (object current in Find.Selector.SelectedObjects)
				{
					Thing thing = current as Thing;
					if (thing != null)
					{
						CellRenderer.RenderCell(thing.InteractionCell, 0.5f);
					}
				}
			}
		}
	}
}
