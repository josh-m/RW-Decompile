using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DropPodUtility
	{
		private static List<List<Thing>> tempList = new List<List<Thing>>();

		public static void MakeDropPodAt(IntVec3 c, DropPodInfo info)
		{
			DropPodIncoming dropPodIncoming = (DropPodIncoming)ThingMaker.MakeThing(ThingDefOf.DropPodIncoming, null);
			dropPodIncoming.contents = info;
			GenSpawn.Spawn(dropPodIncoming, c);
		}

		public static void DropThingsNear(IntVec3 dropCenter, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true)
		{
			foreach (Thing current in things)
			{
				List<Thing> list = new List<Thing>();
				list.Add(current);
				DropPodUtility.tempList.Add(list);
			}
			DropPodUtility.DropThingGroupsNear(dropCenter, DropPodUtility.tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch);
			DropPodUtility.tempList.Clear();
		}

		public static void DropThingGroupsNear(IntVec3 dropCenter, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true)
		{
			foreach (List<Thing> current in thingsGroups)
			{
				IntVec3 intVec;
				if (!DropCellFinder.TryFindDropSpotNear(dropCenter, out intVec, true, canRoofPunch))
				{
					Log.Warning(string.Concat(new object[]
					{
						"DropThingsNear failed to find a place to drop ",
						current.FirstOrDefault<Thing>(),
						" near ",
						dropCenter,
						". Dropping on random square instead."
					}));
					intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(), 1000);
				}
				for (int i = 0; i < current.Count; i++)
				{
					current[i].SetForbidden(true, false);
				}
				if (instaDrop)
				{
					foreach (Thing current2 in current)
					{
						GenPlace.TryPlaceThing(current2, intVec, ThingPlaceMode.Near, null);
					}
				}
				else
				{
					DropPodInfo dropPodInfo = new DropPodInfo();
					foreach (Thing current3 in current)
					{
						dropPodInfo.containedThings.Add(current3);
					}
					dropPodInfo.openDelay = openDelay;
					dropPodInfo.leaveSlag = leaveSlag;
					DropPodUtility.MakeDropPodAt(intVec, dropPodInfo);
				}
			}
		}
	}
}
