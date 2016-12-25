using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_WatchArea : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, base.Map).ToList<IntVec3>());
		}
	}
}
