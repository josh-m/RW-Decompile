using System;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowTradeBeaconRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			Map visibleMap = Find.VisibleMap;
			GenDraw.DrawFieldEdges(Building_OrbitalTradeBeacon.TradeableCellsAround(center, visibleMap));
		}
	}
}
