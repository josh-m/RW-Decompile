using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeverAdjacentTrap : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
		{
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				List<Thing> list = map.thingGrid.ThingsListAt(current);
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing = list[i];
					if (thing != thingToIgnore)
					{
						if ((thing.def.category == ThingCategory.Building && thing.def.building.isTrap) || ((thing.def.IsBlueprint || thing.def.IsFrame) && thing.def.entityDefToBuild is ThingDef && ((ThingDef)thing.def.entityDefToBuild).building.isTrap))
						{
							return "CannotPlaceAdjacentTrap".Translate();
						}
					}
				}
				iterator.MoveNext();
			}
			return true;
		}
	}
}
