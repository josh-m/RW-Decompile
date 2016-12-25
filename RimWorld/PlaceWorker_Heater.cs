using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Heater : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			Room room = center.GetRoom(base.Map);
			if (room != null && !room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList<IntVec3>(), GenTemperature.ColorRoomHot);
			}
		}
	}
}
