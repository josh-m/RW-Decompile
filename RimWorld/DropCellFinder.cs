using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class DropCellFinder
	{
		public static IntVec3 RandomDropSpot()
		{
			return CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable() && !c.Roofed() && !c.Fogged(), 1000);
		}

		public static IntVec3 TradeDropSpot()
		{
			IEnumerable<Building> collection = from b in Find.ListerBuildings.allBuildingsColonist
			where b.def.IsCommsConsole
			select b;
			IEnumerable<Building> enumerable = from b in Find.ListerBuildings.allBuildingsColonist
			where b.def.IsOrbitalTradeBeacon
			select b;
			Building building = enumerable.FirstOrDefault((Building b) => !Find.RoofGrid.Roofed(b.Position) && DropCellFinder.AnyAdjacentGoodDropSpot(b.Position, false, false));
			IntVec3 position;
			if (building != null)
			{
				position = building.Position;
				IntVec3 result;
				if (!DropCellFinder.TryFindDropSpotNear(position, out result, false, false))
				{
					Log.Error("Could find no good TradeDropSpot near dropCenter " + position + ". Using a random standable unfogged cell.");
					result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable() && !c.Fogged(), 1000);
				}
				return result;
			}
			List<Building> list = new List<Building>();
			list.AddRange(enumerable);
			list.AddRange(collection);
			list.RemoveAll(delegate(Building b)
			{
				CompPowerTrader compPowerTrader = b.TryGetComp<CompPowerTrader>();
				return compPowerTrader != null && !compPowerTrader.PowerOn;
			});
			Predicate<IntVec3> validator = (IntVec3 c) => DropCellFinder.IsGoodDropSpot(c, false, false);
			if (!list.Any<Building>())
			{
				list.AddRange(Find.ListerBuildings.allBuildingsColonist);
				list.Shuffle<Building>();
				if (!list.Any<Building>())
				{
					return CellFinderLoose.RandomCellWith(validator, 1000);
				}
			}
			int num = 8;
			while (true)
			{
				for (int i = 0; i < list.Count; i++)
				{
					IntVec3 position2 = list[i].Position;
					if (CellFinder.TryFindRandomCellNear(position2, num, validator, out position))
					{
						return position;
					}
				}
				num = Mathf.RoundToInt((float)num * 1.1f);
				if (num > Find.Map.Size.x)
				{
					goto Block_11;
				}
			}
			return position;
			Block_11:
			Log.Error("Failed to generate trade drop center. Giving random.");
			return CellFinderLoose.RandomCellWith(validator, 1000);
		}

		public static bool TryFindDropSpotNear(IntVec3 center, out IntVec3 result, bool allowFogged, bool canRoofPunch)
		{
			if (DebugViewSettings.drawDestSearch)
			{
				Find.DebugDrawer.FlashCell(center, 1f, "center");
			}
			Predicate<IntVec3> validator = (IntVec3 c) => DropCellFinder.IsGoodDropSpot(c, allowFogged, canRoofPunch) && center.CanReach(c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly);
			int num = 5;
			while (!CellFinder.TryFindRandomCellNear(center, num, validator, out result))
			{
				num += 3;
				if (num > 16)
				{
					result = center;
					return false;
				}
			}
			return true;
		}

		private static bool IsGoodDropSpot(IntVec3 c, bool allowFogged, bool canRoofPunch)
		{
			if (!c.InBounds() || !c.Standable())
			{
				return false;
			}
			if (!DropCellFinder.CanPhysicallyDropInto(c, canRoofPunch))
			{
				if (DebugViewSettings.drawDestSearch)
				{
					Find.DebugDrawer.FlashCell(c, 0f, "phys");
				}
				return false;
			}
			if (Current.ProgramState == ProgramState.MapPlaying && !allowFogged && c.Fogged())
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList();
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def == ThingDefOf.DropPod || thing.def.category == ThingCategory.Skyfaller)
				{
					return false;
				}
				if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(ThingDefOf.DropPod, thing.def))
				{
					return false;
				}
			}
			return true;
		}

		private static bool AnyAdjacentGoodDropSpot(IntVec3 c, bool allowFogged, bool canRoofPunch)
		{
			return DropCellFinder.IsGoodDropSpot(c + IntVec3.North, allowFogged, canRoofPunch) || DropCellFinder.IsGoodDropSpot(c + IntVec3.East, allowFogged, canRoofPunch) || DropCellFinder.IsGoodDropSpot(c + IntVec3.South, allowFogged, canRoofPunch) || DropCellFinder.IsGoodDropSpot(c + IntVec3.West, allowFogged, canRoofPunch);
		}

		public static IntVec3 FindRaiderDropCenterDistant()
		{
			IEnumerable<Thing> enumerable = Find.MapPawns.FreeColonistsSpawned.Cast<Thing>().Concat(Find.ListerBuildings.allBuildingsColonist.Cast<Thing>());
			int num = 0;
			float num2 = 65f;
			IntVec3 intVec;
			while (true)
			{
				intVec = CellFinder.RandomCell();
				num++;
				if (DropCellFinder.CanPhysicallyDropInto(intVec, true))
				{
					if (num > 300)
					{
						break;
					}
					if (!intVec.Roofed())
					{
						num2 -= 0.2f;
						bool flag = false;
						foreach (Thing current in enumerable)
						{
							if ((intVec - current.Position).LengthHorizontalSquared < num2 * num2)
							{
								flag = true;
								break;
							}
						}
						if (!flag && intVec.CanReachColony())
						{
							return intVec;
						}
					}
				}
			}
			return intVec;
		}

		public static bool TryFindRaiderDropCenterClose(out IntVec3 spot)
		{
			int num = 0;
			while (true)
			{
				IntVec3 root = IntVec3.Invalid;
				if (Find.MapPawns.FreeColonistsSpawnedCount > 0)
				{
					root = Find.MapPawns.FreeColonistsSpawned.RandomElement<Pawn>().Position;
				}
				else
				{
					List<Building> allBuildingsColonist = Find.ListerBuildings.allBuildingsColonist;
					for (int i = 0; i < allBuildingsColonist.Count; i++)
					{
						if (DropCellFinder.TryFindDropSpotNear(allBuildingsColonist[i].Position, out root, true, true))
						{
							break;
						}
					}
					if (!root.IsValid)
					{
						root = DropCellFinder.RandomDropSpot();
					}
				}
				spot = CellFinder.RandomClosewalkCellNear(root, 10);
				if (DropCellFinder.CanPhysicallyDropInto(spot, true))
				{
					break;
				}
				num++;
				if (num > 300)
				{
					goto Block_5;
				}
			}
			return true;
			Block_5:
			spot = CellFinderLoose.RandomCellWith((IntVec3 c) => DropCellFinder.CanPhysicallyDropInto(c, true), 1000);
			return false;
		}

		private static bool CanPhysicallyDropInto(IntVec3 c, bool canRoofPunch)
		{
			if (!c.Walkable())
			{
				return false;
			}
			RoofDef roof = c.GetRoof();
			if (roof != null)
			{
				if (!canRoofPunch)
				{
					return false;
				}
				if (roof.isThickRoof)
				{
					return false;
				}
			}
			return true;
		}
	}
}
