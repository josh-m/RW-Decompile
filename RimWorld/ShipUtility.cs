using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ShipUtility
	{
		private static List<Building> closedSet = new List<Building>();

		private static List<Building> openSet = new List<Building>();

		[DebuggerHidden]
		public static IEnumerable<string> LaunchFailReasons(Building rootBuilding)
		{
			List<Building> shipParts = ShipUtility.ShipBuildingsAttachedTo(rootBuilding).ToList<Building>();
			foreach (ThingDef partDef in new List<ThingDef>
			{
				ThingDefOf.Ship_CryptosleepCasket,
				ThingDefOf.Ship_ComputerCore,
				ThingDefOf.Ship_Reactor,
				ThingDefOf.Ship_Engine
			})
			{
				if (!shipParts.Any((Building pa) => pa.def == this.<partDef>__3))
				{
					yield return "ShipReportMissingPart".Translate() + ": " + partDef.label;
				}
			}
			bool fullPodFound = false;
			foreach (Building part in shipParts)
			{
				if (part.def == ThingDefOf.Ship_CryptosleepCasket)
				{
					Building_CryptosleepCasket pod = part as Building_CryptosleepCasket;
					if (pod != null && pod.HasAnyContents)
					{
						fullPodFound = true;
						break;
					}
				}
			}
			if (!fullPodFound)
			{
				yield return "ShipReportNoFullPods".Translate();
			}
		}

		public static List<Building> ShipBuildingsAttachedTo(Building root)
		{
			if (root == null || root.Destroyed)
			{
				return new List<Building>();
			}
			ShipUtility.closedSet.Clear();
			ShipUtility.openSet.Clear();
			ShipUtility.openSet.Add(root);
			while (ShipUtility.openSet.Count > 0)
			{
				Building building = ShipUtility.openSet[ShipUtility.openSet.Count - 1];
				ShipUtility.openSet.Remove(building);
				ShipUtility.closedSet.Add(building);
				foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(building))
				{
					Building edifice = current.GetEdifice(building.Map);
					if (edifice != null && edifice.def.building.shipPart && !ShipUtility.closedSet.Contains(edifice) && !ShipUtility.openSet.Contains(edifice))
					{
						ShipUtility.openSet.Add(edifice);
					}
				}
			}
			return ShipUtility.closedSet;
		}
	}
}
