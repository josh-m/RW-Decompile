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
			SlotGroup slotGroup = Find.SlotGroupManager.SlotGroupAt(t.Position);
			return slotGroup != null;
		}

		public static bool IsInValidStorage(this Thing t)
		{
			SlotGroup slotGroup = t.Position.GetSlotGroup();
			return slotGroup != null && slotGroup.Settings.AllowedToAccept(t);
		}

		public static bool IsInValidBestStorage(this Thing t)
		{
			SlotGroup slotGroup = t.Position.GetSlotGroup();
			IntVec3 intVec;
			return slotGroup != null && slotGroup.Settings.AllowedToAccept(t) && !StoreUtility.TryFindBestBetterStoreCellFor(t, null, slotGroup.Settings.Priority, Faction.OfPlayer, out intVec, false);
		}

		public static Building StoringBuilding(this Thing t)
		{
			if (!t.Spawned)
			{
				return null;
			}
			SlotGroup slotGroup = t.Position.GetSlotGroup();
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

		public static SlotGroup GetSlotGroup(this IntVec3 c)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return null;
			}
			return Find.SlotGroupManager.SlotGroupAt(c);
		}

		public static Thing GetStorable(this IntVec3 c)
		{
			foreach (Thing current in Find.ThingGrid.ThingsAt(c))
			{
				if (current.def.EverStoreable)
				{
					return current;
				}
			}
			return null;
		}

		public static bool IsValidStorageFor(this IntVec3 c, Thing storable)
		{
			if (!StoreUtility.NoStorageBlockersIn(c, storable))
			{
				return false;
			}
			SlotGroup slotGroup = c.GetSlotGroup();
			return slotGroup != null && slotGroup.Settings.AllowedToAccept(storable);
		}

		private static bool NoStorageBlockersIn(IntVec3 c, Thing thing)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
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

		public static bool TryFindBestBetterStoreCellFor(Thing t, Pawn carrier, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, bool needAccurateResult = true)
		{
			List<SlotGroup> allGroupsListInPriorityOrder = Find.SlotGroupManager.AllGroupsListInPriorityOrder;
			if (allGroupsListInPriorityOrder.Count == 0)
			{
				foundCell = IntVec3.Invalid;
				return false;
			}
			IntVec3 positionHeld = t.PositionHeld;
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
						float lengthHorizontalSquared = (positionHeld - intVec2).LengthHorizontalSquared;
						if (lengthHorizontalSquared <= num)
						{
							if (StoreUtility.IsGoodStoreCell(intVec2, t, carrier, faction))
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

		public static bool IsGoodStoreCell(IntVec3 c, Thing t, Pawn carrier, Faction faction)
		{
			if (carrier != null && c.IsForbidden(carrier))
			{
				return false;
			}
			if (!StoreUtility.NoStorageBlockersIn(c, t))
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
			else if (Find.Reservations.IsReserved(c, faction))
			{
				return false;
			}
			return !c.ContainsStaticFire() && (carrier == null || t.PositionHeld.CanReach(c, PathEndMode.ClosestTouch, TraverseParms.For(carrier, Danger.Deadly, TraverseMode.ByPawn, false)));
		}
	}
}
