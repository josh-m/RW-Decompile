using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PowerNetMaker
	{
		private static HashSet<Building> closedSet = new HashSet<Building>();

		private static HashSet<Building> openSet = new HashSet<Building>();

		private static HashSet<Building> currentSet = new HashSet<Building>();

		private static IEnumerable<CompPower> ContiguousPowerBuildings(Building root)
		{
			PowerNetMaker.closedSet.Clear();
			PowerNetMaker.currentSet.Clear();
			PowerNetMaker.openSet.Add(root);
			do
			{
				foreach (Building current in PowerNetMaker.openSet)
				{
					PowerNetMaker.closedSet.Add(current);
				}
				HashSet<Building> hashSet = PowerNetMaker.currentSet;
				PowerNetMaker.currentSet = PowerNetMaker.openSet;
				PowerNetMaker.openSet = hashSet;
				PowerNetMaker.openSet.Clear();
				foreach (Building current2 in PowerNetMaker.currentSet)
				{
					foreach (IntVec3 current3 in GenAdj.CellsAdjacentCardinal(current2))
					{
						List<Thing> thingList = current3.GetThingList(current2.Map);
						for (int i = 0; i < thingList.Count; i++)
						{
							Building building = thingList[i] as Building;
							if (building != null)
							{
								if (building.TransmitsPowerNow)
								{
									if (!PowerNetMaker.openSet.Contains(building) && !PowerNetMaker.currentSet.Contains(building) && !PowerNetMaker.closedSet.Contains(building))
									{
										PowerNetMaker.openSet.Add(building);
										break;
									}
								}
							}
						}
					}
				}
			}
			while (PowerNetMaker.openSet.Count > 0);
			return from b in PowerNetMaker.closedSet
			select b.PowerComp;
		}

		public static PowerNet NewPowerNetStartingFrom(Building root)
		{
			return new PowerNet(PowerNetMaker.ContiguousPowerBuildings(root));
		}

		public static void UpdateVisualLinkagesFor(PowerNet net)
		{
		}
	}
}
