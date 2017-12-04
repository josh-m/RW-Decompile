using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_WatchArea : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			Map visibleMap = Find.VisibleMap;
			GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, visibleMap).ToList<IntVec3>());
		}
	}
}
