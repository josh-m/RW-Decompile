using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeedsFuelingPort : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			if (FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(center, map) == null)
			{
				return "MustPlaceNearFuelingPort".Translate();
			}
			return true;
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			Map visibleMap = Find.VisibleMap;
			List<Building> allBuildingsColonist = visibleMap.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building building = allBuildingsColonist[i];
				if (building.def.building.hasFuelingPort && !Find.Selector.IsSelected(building) && FuelingPortUtility.GetFuelingPortCell(building).Standable(visibleMap))
				{
					PlaceWorker_FuelingPort.DrawFuelingPortCell(building.Position, building.Rotation);
				}
			}
		}
	}
}
