using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeverAdjacentUnstandable : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			GenDraw.DrawFieldEdges(GenAdj.OccupiedRect(center, rot, def.size).ExpandedBy(1).Cells.ToList<IntVec3>(), Color.white);
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Thing thingToIgnore = null)
		{
			CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				List<Thing> list = base.Map.thingGrid.ThingsListAt(current);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != thingToIgnore && list[i].def.passability != Traversability.Standable)
					{
						return "MustPlaceAdjacentStandable".Translate();
					}
				}
				iterator.MoveNext();
			}
			return true;
		}
	}
}
