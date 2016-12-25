using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Cooler : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			IntVec3 intVec = center + IntVec3.South.RotatedBy(rot);
			IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec
			}, GenTemperature.ColorSpotCold);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec2
			}, GenTemperature.ColorSpotHot);
			Room room = intVec2.GetRoom(base.Map);
			Room room2 = intVec.GetRoom(base.Map);
			if (room != null && room2 != null)
			{
				if (room == room2 && !room.UsesOutdoorTemperature)
				{
					GenDraw.DrawFieldEdges(room.Cells.ToList<IntVec3>(), new Color(1f, 0.7f, 0f, 0.5f));
				}
				else
				{
					if (!room.UsesOutdoorTemperature)
					{
						GenDraw.DrawFieldEdges(room.Cells.ToList<IntVec3>(), GenTemperature.ColorRoomHot);
					}
					if (!room2.UsesOutdoorTemperature)
					{
						GenDraw.DrawFieldEdges(room2.Cells.ToList<IntVec3>(), GenTemperature.ColorRoomCold);
					}
				}
			}
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Thing thingToIgnore = null)
		{
			IntVec3 c = center + IntVec3.South.RotatedBy(rot);
			IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
			if (c.Impassable(base.Map) || c2.Impassable(base.Map))
			{
				return "MustPlaceCoolerWithFreeSpaces".Translate();
			}
			return true;
		}
	}
}
