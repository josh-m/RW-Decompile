using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public static class HaulAIUtility
	{
		private static string ForbiddenLowerTrans;

		private static string ForbiddenOutsideAllowedAreaLowerTrans;

		private static string PrisonerRoomLowerTrans;

		private static string BurningLowerTrans;

		private static string NoEmptyPlaceLowerTrans;

		private static List<IntVec3> candidates = new List<IntVec3>();

		public static void Reset()
		{
			HaulAIUtility.ForbiddenLowerTrans = "ForbiddenLower".Translate();
			HaulAIUtility.ForbiddenOutsideAllowedAreaLowerTrans = "ForbiddenOutsideAllowedAreaLower".Translate();
			HaulAIUtility.PrisonerRoomLowerTrans = "PrisonerRoomLower".Translate();
			HaulAIUtility.BurningLowerTrans = "BurningLower".Translate();
			HaulAIUtility.NoEmptyPlaceLowerTrans = "NoEmptyPlaceLower".Translate();
		}

		public static bool PawnCanAutomaticallyHaul(Pawn p, Thing t)
		{
			if (!t.def.EverHaulable)
			{
				return false;
			}
			if (t.IsForbidden(p))
			{
				if (!t.Position.InAllowedArea(p))
				{
					JobFailReason.Is(HaulAIUtility.ForbiddenOutsideAllowedAreaLowerTrans);
				}
				else
				{
					JobFailReason.Is(HaulAIUtility.ForbiddenLowerTrans);
				}
				return false;
			}
			UnfinishedThing unfinishedThing = t as UnfinishedThing;
			if (unfinishedThing != null && unfinishedThing.BoundBill != null)
			{
				return false;
			}
			if (!t.def.alwaysHaulable && t.Map.designationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInValidStorage())
			{
				return false;
			}
			if (!p.CanReserveAndReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger(), 1))
			{
				return false;
			}
			if (t.def.IsNutritionGivingIngestible && t.def.ingestible.HumanEdible && !t.IsSociallyProper(p))
			{
				JobFailReason.Is(HaulAIUtility.PrisonerRoomLowerTrans);
				return false;
			}
			if (t.IsBurning())
			{
				JobFailReason.Is(HaulAIUtility.BurningLowerTrans);
				return false;
			}
			return true;
		}

		public static bool PawnCanAutomaticallyHaulFast(Pawn p, Thing t)
		{
			UnfinishedThing unfinishedThing = t as UnfinishedThing;
			if (unfinishedThing != null && unfinishedThing.BoundBill != null)
			{
				return false;
			}
			if (!p.CanReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger(), false, TraverseMode.ByPawn))
			{
				return false;
			}
			if (!p.Map.reservationManager.CanReserve(p, t, 1))
			{
				return false;
			}
			if (t.def.IsNutritionGivingIngestible && t.def.ingestible.HumanEdible && !t.IsSociallyProper(p, false, true))
			{
				JobFailReason.Is(HaulAIUtility.PrisonerRoomLowerTrans);
				return false;
			}
			if (t.IsBurning())
			{
				JobFailReason.Is(HaulAIUtility.BurningLowerTrans);
				return false;
			}
			return true;
		}

		public static Job HaulToStorageJob(Pawn p, Thing t)
		{
			StoragePriority currentPriority = HaulAIUtility.StoragePriorityAtFor(t.Position, t);
			IntVec3 storeCell;
			if (!StoreUtility.TryFindBestBetterStoreCellFor(t, p, p.Map, currentPriority, p.Faction, out storeCell, true))
			{
				JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
				return null;
			}
			return HaulAIUtility.HaulMaxNumToCellJob(p, t, storeCell, false);
		}

		public static Job HaulMaxNumToCellJob(Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
		{
			Job job = new Job(JobDefOf.HaulToCell, t, storeCell);
			SlotGroup slotGroup = p.Map.slotGroupManager.SlotGroupAt(storeCell);
			if (slotGroup != null)
			{
				Thing thing = p.Map.thingGrid.ThingAt(storeCell, t.def);
				if (thing != null)
				{
					job.count = t.def.stackLimit;
					if (fitInStoreCell)
					{
						job.count -= thing.stackCount;
					}
				}
				else
				{
					job.count = 99999;
				}
				int num = 0;
				float statValue = p.GetStatValue(StatDefOf.CarryingCapacity, true);
				List<IntVec3> cellsList = slotGroup.CellsList;
				for (int i = 0; i < cellsList.Count; i++)
				{
					if (StoreUtility.IsGoodStoreCell(cellsList[i], p.Map, t, p, p.Faction))
					{
						Thing thing2 = p.Map.thingGrid.ThingAt(cellsList[i], t.def);
						if (thing2 != null && thing2 != t)
						{
							num += Mathf.Max(t.def.stackLimit - thing2.stackCount, 0);
						}
						else
						{
							num += t.def.stackLimit;
						}
						if (num >= job.count || (float)num >= statValue)
						{
							break;
						}
					}
				}
				job.count = Mathf.Min(job.count, num);
			}
			else
			{
				job.count = 99999;
			}
			job.haulOpportunisticDuplicates = true;
			job.haulMode = HaulMode.ToCellStorage;
			return job;
		}

		public static StoragePriority StoragePriorityAtFor(IntVec3 c, Thing t)
		{
			if (!t.Spawned)
			{
				return StoragePriority.Unstored;
			}
			SlotGroup slotGroup = t.Map.slotGroupManager.SlotGroupAt(c);
			if (slotGroup == null || !slotGroup.Settings.AllowedToAccept(t))
			{
				return StoragePriority.Unstored;
			}
			return slotGroup.Settings.Priority;
		}

		public static bool CanHaulAside(Pawn p, Thing t, out IntVec3 storeCell)
		{
			storeCell = IntVec3.Invalid;
			return t.def.EverHaulable && !t.IsBurning() && p.CanReserveAndReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger(), 1) && HaulAIUtility.TryFindSpotToPlaceHaulableCloseTo(t, p, t.PositionHeld, out storeCell);
		}

		public static Job HaulAsideJobFor(Pawn p, Thing t)
		{
			IntVec3 c;
			if (!HaulAIUtility.CanHaulAside(p, t, out c))
			{
				return null;
			}
			return new Job(JobDefOf.HaulToCell, t, c)
			{
				count = 99999,
				haulOpportunisticDuplicates = false,
				haulMode = HaulMode.ToCellNonStorage,
				ignoreDesignations = true
			};
		}

		private static bool TryFindSpotToPlaceHaulableCloseTo(Thing haulable, Pawn worker, IntVec3 center, out IntVec3 spot)
		{
			Region region = center.GetRegion(worker.Map);
			if (region == null)
			{
				spot = center;
				return false;
			}
			TraverseParms traverseParms = TraverseParms.For(worker, Danger.Deadly, TraverseMode.ByPawn, false);
			IntVec3 foundCell = IntVec3.Invalid;
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, false), delegate(Region r)
			{
				HaulAIUtility.candidates.Clear();
				HaulAIUtility.candidates.AddRange(r.Cells);
				HaulAIUtility.candidates.Sort((IntVec3 a, IntVec3 b) => a.DistanceToSquared(center).CompareTo(b.DistanceToSquared(center)));
				for (int i = 0; i < HaulAIUtility.candidates.Count; i++)
				{
					IntVec3 intVec = HaulAIUtility.candidates[i];
					if (HaulAIUtility.HaulablePlaceValidator(haulable, worker, intVec))
					{
						foundCell = intVec;
						return true;
					}
				}
				return false;
			}, 100);
			if (foundCell.IsValid)
			{
				spot = foundCell;
				return true;
			}
			spot = center;
			return false;
		}

		private static bool HaulablePlaceValidator(Thing haulable, Pawn worker, IntVec3 c)
		{
			if (!worker.CanReserveAndReach(c, PathEndMode.OnCell, worker.NormalMaxDanger(), 1))
			{
				return false;
			}
			if (GenPlace.HaulPlaceBlockerIn(haulable, c, worker.Map, true) != null)
			{
				return false;
			}
			if (!c.Standable(worker.Map))
			{
				return false;
			}
			if (c == haulable.Position && haulable.Spawned)
			{
				return false;
			}
			if (haulable != null && haulable.def.BlockPlanting)
			{
				Zone zone = worker.Map.zoneManager.ZoneAt(c);
				if (zone is Zone_Growing)
				{
					return false;
				}
			}
			if (haulable.def.passability != Traversability.Standable)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c2 = c + GenAdj.AdjacentCells[i];
					if (worker.Map.designationManager.DesignationAt(c2, DesignationDefOf.Mine) != null)
					{
						return false;
					}
				}
			}
			Building edifice = c.GetEdifice(worker.Map);
			if (edifice != null)
			{
				Building_Trap building_Trap = edifice as Building_Trap;
				if (building_Trap != null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
