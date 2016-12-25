using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowDeepResources : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
		{
			List<Building> allBuildingsColonist = base.Map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building thing = allBuildingsColonist[i];
				CompDeepScanner compDeepScanner = thing.TryGetComp<CompDeepScanner>();
				if (compDeepScanner != null)
				{
					CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
					if (compPowerTrader == null || compPowerTrader.PowerOn)
					{
						base.Map.deepResourceGrid.DeepResourceGridDraw(true);
					}
				}
			}
		}
	}
}
