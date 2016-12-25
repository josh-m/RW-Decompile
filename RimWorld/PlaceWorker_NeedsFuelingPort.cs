using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeedsFuelingPort : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Thing thingToIgnore = null)
		{
			if (FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(center, Find.VisibleMap) == null)
			{
				return "MustPlaceNearFuelingPort".Translate();
			}
			return true;
		}

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			List<Building> allBuildingsColonist = Find.VisibleMap.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building building = allBuildingsColonist[i];
				if (building.def.building.hasFuelingPort && !Find.Selector.IsSelected(building) && FuelingPortUtility.GetFuelingPortCell(building).Standable(Find.VisibleMap))
				{
					PlaceWorker_FuelingPort.DrawFuelingPortCell(building.Position, building.Rotation);
				}
			}
		}
	}
}
