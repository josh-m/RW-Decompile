using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class PartyUtility
	{
		private const float PartyAreaRadiusIfNotWholeRoom = 10f;

		private const int MaxRoomCellsCountToUseWholeRoom = 324;

		public static bool AcceptableGameConditionsToStartParty(Map map)
		{
			if (!PartyUtility.AcceptableGameConditionsToContinueParty(map))
			{
				return false;
			}
			if (GenLocalDate.HourInteger(map) < 4 || GenLocalDate.HourInteger(map) > 21)
			{
				return false;
			}
			if (GatheringsUtility.AnyLordJobPreventsNewGatherings(map))
			{
				return false;
			}
			if (map.dangerWatcher.DangerRating != StoryDanger.None)
			{
				return false;
			}
			int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
			if (freeColonistsSpawnedCount < 4)
			{
				return false;
			}
			int num = 0;
			foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
			{
				if (current.health.hediffSet.BleedRateTotal > 0f)
				{
					return false;
				}
				if (current.Drafted)
				{
					num++;
				}
			}
			return (float)num / (float)freeColonistsSpawnedCount < 0.5f && PartyUtility.EnoughPotentialGuestsToStartParty(map, null);
		}

		public static bool AcceptableGameConditionsToContinueParty(Map map)
		{
			return map.dangerWatcher.DangerRating != StoryDanger.High;
		}

		public static bool EnoughPotentialGuestsToStartParty(Map map, IntVec3? partySpot = null)
		{
			int num = Mathf.RoundToInt((float)map.mapPawns.FreeColonistsSpawnedCount * 0.65f);
			num = Mathf.Clamp(num, 2, 10);
			int num2 = 0;
			foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
			{
				if (PartyUtility.ShouldPawnKeepPartying(current))
				{
					if (!partySpot.HasValue || !partySpot.Value.IsForbidden(current))
					{
						if (!partySpot.HasValue || current.CanReach(partySpot.Value, PathEndMode.Touch, Danger.Some, false, TraverseMode.ByPawn))
						{
							num2++;
						}
					}
				}
			}
			return num2 >= num;
		}

		public static Pawn FindRandomPartyOrganizer(Faction faction, Map map)
		{
			Predicate<Pawn> validator = (Pawn x) => x.RaceProps.Humanlike && !x.InBed() && !x.InMentalState && x.GetLord() == null && PartyUtility.ShouldPawnKeepPartying(x);
			Pawn result;
			if ((from x in map.mapPawns.SpawnedPawnsInFaction(faction)
			where validator(x)
			select x).TryRandomElement(out result))
			{
				return result;
			}
			return null;
		}

		public static bool ShouldPawnKeepPartying(Pawn p)
		{
			return (p.timetable == null || p.timetable.CurrentAssignment.allowJoy) && GatheringsUtility.ShouldGuestKeepAttendingGathering(p);
		}

		public static bool InPartyArea(IntVec3 cell, IntVec3 partySpot, Map map)
		{
			if (PartyUtility.UseWholeRoomAsPartyArea(partySpot, map) && cell.GetRoom(map, RegionType.Set_Passable) == partySpot.GetRoom(map, RegionType.Set_Passable))
			{
				return true;
			}
			if (!cell.InHorDistOf(partySpot, 10f))
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.None, false);
			if (edifice != null)
			{
				return map.reachability.CanReach(partySpot, edifice, PathEndMode.ClosestTouch, traverseParams);
			}
			return map.reachability.CanReach(partySpot, cell, PathEndMode.ClosestTouch, traverseParams);
		}

		public static bool TryFindRandomCellInPartyArea(Pawn pawn, out IntVec3 result)
		{
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false);
			if (PartyUtility.UseWholeRoomAsPartyArea(cell, pawn.Map))
			{
				Room room = cell.GetRoom(pawn.Map, RegionType.Set_Passable);
				return (from x in room.Cells
				where validator(x)
				select x).TryRandomElement(out result);
			}
			return CellFinder.TryFindRandomReachableCellNear(cell, pawn.Map, 10f, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => validator(x), null, out result, 10);
		}

		public static bool UseWholeRoomAsPartyArea(IntVec3 partySpot, Map map)
		{
			Room room = partySpot.GetRoom(map, RegionType.Set_Passable);
			return room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount <= 324;
		}
	}
}
