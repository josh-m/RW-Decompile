using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Vent : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			IntVec3 intVec = center + IntVec3.South.RotatedBy(rot);
			IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec
			}, Color.white);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec2
			}, Color.white);
			Room room = intVec2.GetRoom();
			Room room2 = intVec.GetRoom();
			if (room != null && room2 != null)
			{
				if (room == room2 && !room.UsesOutdoorTemperature)
				{
					GenDraw.DrawFieldEdges(room.Cells.ToList<IntVec3>(), Color.white);
				}
				else
				{
					if (!room.UsesOutdoorTemperature)
					{
						GenDraw.DrawFieldEdges(room.Cells.ToList<IntVec3>(), Color.white);
					}
					if (!room2.UsesOutdoorTemperature)
					{
						GenDraw.DrawFieldEdges(room2.Cells.ToList<IntVec3>(), Color.white);
					}
				}
			}
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot)
		{
			IntVec3 c = center + IntVec3.South.RotatedBy(rot);
			IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
			if (c.Impassable() || c2.Impassable())
			{
				return "MustPlaceVentWithFreeSpaces".Translate();
			}
			return true;
		}
	}
}
