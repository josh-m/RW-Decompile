using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Heater : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			RoomGroup roomGroup = center.GetRoomGroup(base.Map);
			if (roomGroup != null && !roomGroup.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(roomGroup.Cells.ToList<IntVec3>(), GenTemperature.ColorRoomHot);
			}
		}
	}
}
