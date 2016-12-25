using System;
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

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot)
		{
			foreach (IntVec3 current in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1).Cells)
			{
				if (!current.Standable())
				{
					return "MustPlaceAdjacentStandable".Translate();
				}
			}
			return true;
		}
	}
}
