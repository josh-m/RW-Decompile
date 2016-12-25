using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class StoreUtility
	{
		public static bool IsInAnyStorage(this Thing t)
		{
			if (!t.Spawned)
			{
				return false;
			}
			SlotGroup slotGroup = t.Map.slotGroupManager.SlotGroupAt(t.Position);
			return slotGroup != null;
		}

		public static bool IsInValidStorage(this Thing t)
		{
			if (!t.Spawned)
			{
				return false;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			return slotGroup != null && slotGroup.Settings.AllowedToAccept(t);
		}

		public static bool IsInValidBestStorage(this Thing t)
		{
			if (!t.Spawned)
			{
				return false;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			IntVec3 intVec;
			return slotGroup != null && slotGroup.Settings.AllowedToAccept(t) && !StoreUtility.TryFindBestBetterStoreCellFor(t, null, t.Map, slotGroup.Settings.Priority, Faction.OfPlayer, out intVec, false);
		}

		public static Building StoringBuilding(this Thing t)
		{
			if (!t.Spawned)
			{
				return null;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			if (slotGroup == null)
			{
				return null;
			}
			Building building = slotGroup.parent as Building;
			if (building != null)
			{
				return building;
			}
			return null;
		}

		public static SlotGroup GetSlotGroup(this Thing thing)
		{
			if (!thing.Spawned)
			{
				return null;
			}
			return thing.Position.GetSlotGroup(thing.Map);
		}

		public static SlotGroup GetSlotGroup(this IntVec3 c, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return null;
			}
			return map.slotGroupManager.SlotGroupAt(c);
		}

		public static Thing GetStorable(this IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.EverStoreable)
				{
					return thingList[i];
				}
			}
			return null;
		}

		public static bool IsValidStorageFor(this IntVec3 c, Map map, Thing storable)
		{
			if (!StoreUtility.NoStorageBlockersIn(c, map, storable))
			{
				return false;
			}
			SlotGroup slotGroup = c.GetSlotGroup(map);
			return slotGroup != null && slotGroup.Settings.AllowedToAccept(storable);
		}

		private static bool NoStorageBlockersIn(IntVec3 c, Map map, Thing thing)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing2.def.EverStoreable)
				{
					if (thing2.def != thing.def)
					{
						return false;
					}
					if (thing2.stackCount >= thing.def.stackLimit)
					{
						return false;
					}
				}
				if (thing2.def.entityDefToBuild != null && thing2.def.entityDefToBuild.passability != Traversability.Standable)
				{
					return false;
				}
				if (thing2.def.surfaceType == SurfaceType.None && thing2.def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryFindBestBetterStoreCellFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, bool needAccurateResult = true)
		{
			List<SlotGroup> allGroupsListInPriorityOrder = map.slotGroupManager.AllGroupsListInPriorityOrder;
			if (allGroupsListInPriorityOrder.Count == 0)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			IntVec3 a = (t.MapHeld == null) ? carrier.PositionHeld : t.PositionHeld;
			StoragePriority storagePriority = currentPriority;
			float num = 2.14748365E+09f;
			IntVec3 intVec = default(IntVec3);
			bool flag = false;
			int count = allGroupsListInPriorityOrder.Count;
			for (int i = 0; i < count; i++)
			{
				SlotGroup slotGroup = allGroupsListInPriorityOrder[i];
				StoragePriority priority = slotGroup.Settings.Priority;
				if (priority < storagePriority || priority <= currentPriority)
				{
					break;
				}
				if (slotGroup.Settings.AllowedToAccept(t))
				{
					List<IntVec3> cellsList = slotGroup.CellsList;
					int count2 = cellsList.Count;
					int num2;
					if (needAccurateResult)
					{
						num2 = Mathf.FloorToInt((float)count2 * Rand.Range(0.005f, 0.018f));
					}
					else
					{
						num2 = 0;
					}
					for (int j = 0; j < count2; j++)
					{
						IntVec3 intVec2 = cellsList[j];
						float lengthHorizontalSquared = (a - intVec2).LengthHorizontalSquared;
						if (lengthHorizontalSquared <= num)
						{
							if (StoreUtility.IsGoodStoreCell(intVec2, map, t, carrier, faction))
							{
								flag = true;
								intVec = intVec2;
								num = lengthHorizontalSquared;
								storagePriority = priority;
								if (j >= num2)
								{
									break;
								}
							}
						}
					}
				}
			}
			if (!flag)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			foundCell = intVec;
			return true;
		}

		public static bool IsGoodStoreCell(IntVec3 c, Map map, Thing t, Pawn carrier, Faction faction)
		{
			if (carrier != null && c.IsForbidden(carrier))
			{
				return false;
			}
			if (!StoreUtility.NoStorageBlockersIn(c, map, t))
			{
				return false;
			}
			if (carrier != null)
			{
				if (!carrier.CanReserve(c, 1))
				{
					return false;
				}
			}
			else if (map.reservationManager.IsReserved(c, faction))
			{
				return false;
			}
			return !c.ContainsStaticFire(map) && (carrier == null || carrier.Map.reachability.CanReach((t.MapHeld == null) ? carrier.PositionHeld : t.PositionHeld, c, PathEndMode.ClosestTouch, TraverseParms.For(carrier, Danger.Deadly, TraverseMode.ByPawn, false)));
		}

		public static bool TryFindStoreCellNearColonyDesperate(Thing item, Pawn carrier, out IntVec3 storeCell)
		{
			if (StoreUtility.TryFindBestBetterStoreCellFor(item, carrier, carrier.Map, StoragePriority.Unstored, carrier.Faction, out storeCell, true))
			{
				return true;
			}
			for (int i = -4; i < 20; i++)
			{
				int num = (i >= 0) ? i : Rand.RangeInclusive(0, 4);
				IntVec3 intVec = carrier.Position + GenRadial.RadialPattern[num];
				if (intVec.InBounds(carrier.Map) && carrier.Map.areaManager.Home[intVec] && carrier.CanReach(intVec, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn) && intVec.GetSlotGroup(carrier.Map) == null && StoreUtility.IsGoodStoreCell(intVec, carrier.Map, item, carrier, carrier.Faction))
				{
					storeCell = intVec;
					return true;
				}
			}
			if (RCellFinder.TryFindRandomSpotJustOutsideColony(carrier.Position, carrier.Map, carrier, out storeCell, (IntVec3 x) => x.GetSlotGroup(carrier.Map) == null && StoreUtility.IsGoodStoreCell(x, carrier.Map, item, carrier, carrier.Faction)))
			{
				return true;
			}
			storeCell = IntVec3.Invalid;
			return false;
		}
	}
}
