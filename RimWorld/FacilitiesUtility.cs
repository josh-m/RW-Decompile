using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FacilitiesUtility
	{
		private static int RegionsToSearch = (1 + 2 * Mathf.CeilToInt(0.6666667f)) * (1 + 2 * Mathf.CeilToInt(0.6666667f));

		private static HashSet<Region> visited = new HashSet<Region>();

		private static bool working;

		public static void NotifyFacilitiesAboutChangedLOSBlockers(List<Region> affectedRegions)
		{
			if (FacilitiesUtility.working)
			{
				Log.Warning("Tried to update facilities while already updating.");
				return;
			}
			FacilitiesUtility.working = true;
			try
			{
				ProfilerThreadCheck.BeginSample("NotifyFacilitiesAboutChangedLOSBlockers()");
				FacilitiesUtility.visited.Clear();
				for (int i = 0; i < affectedRegions.Count; i++)
				{
					if (!FacilitiesUtility.visited.Contains(affectedRegions[i]))
					{
						RegionTraverser.BreadthFirstTraverse(affectedRegions[i], (Region from, Region r) => !FacilitiesUtility.visited.Contains(r), delegate(Region x)
						{
							FacilitiesUtility.visited.Add(x);
							List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
							for (int j = 0; j < list.Count; j++)
							{
								CompFacility compFacility = list[j].TryGetComp<CompFacility>();
								CompAffectedByFacilities compAffectedByFacilities = list[j].TryGetComp<CompAffectedByFacilities>();
								if (compFacility != null)
								{
									compFacility.Notify_LOSBlockerSpawnedOrDespawned();
								}
								if (compAffectedByFacilities != null)
								{
									compAffectedByFacilities.Notify_LOSBlockerSpawnedOrDespawned();
								}
							}
							return false;
						}, FacilitiesUtility.RegionsToSearch);
					}
				}
				ProfilerThreadCheck.EndSample();
			}
			finally
			{
				FacilitiesUtility.working = false;
				FacilitiesUtility.visited.Clear();
			}
		}
	}
}
