using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DropPodUtility
	{
		private static List<List<Thing>> tempList = new List<List<Thing>>();

		public static void MakeDropPodAt(IntVec3 c, Map map, ActiveDropPodInfo info)
		{
			DropPodIncoming dropPodIncoming = (DropPodIncoming)ThingMaker.MakeThing(ThingDefOf.DropPodIncoming, null);
			dropPodIncoming.Contents = info;
			GenSpawn.Spawn(dropPodIncoming, c, map);
		}

		public static void DropThingsNear(IntVec3 dropCenter, Map map, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true)
		{
			foreach (Thing current in things)
			{
				List<Thing> list = new List<Thing>();
				list.Add(current);
				DropPodUtility.tempList.Add(list);
			}
			DropPodUtility.DropThingGroupsNear(dropCenter, map, DropPodUtility.tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch);
			DropPodUtility.tempList.Clear();
		}

		public static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true)
		{
			foreach (List<Thing> current in thingsGroups)
			{
				IntVec3 intVec;
				if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out intVec, true, canRoofPunch))
				{
					Log.Warning(string.Concat(new object[]
					{
						"DropThingsNear failed to find a place to drop ",
						current.FirstOrDefault<Thing>(),
						" near ",
						dropCenter,
						". Dropping on random square instead."
					}));
					intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map), map, 1000);
				}
				for (int i = 0; i < current.Count; i++)
				{
					current[i].SetForbidden(true, false);
				}
				if (instaDrop)
				{
					foreach (Thing current2 in current)
					{
						GenPlace.TryPlaceThing(current2, intVec, map, ThingPlaceMode.Near, null);
					}
				}
				else
				{
					ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
					foreach (Thing current3 in current)
					{
						activeDropPodInfo.innerContainer.TryAdd(current3, true);
					}
					activeDropPodInfo.openDelay = openDelay;
					activeDropPodInfo.leaveSlag = leaveSlag;
					DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo);
				}
			}
		}
	}
}
