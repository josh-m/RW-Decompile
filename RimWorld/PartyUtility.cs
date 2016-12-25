using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class PartyUtility
	{
		private const float PartyAreaRadiusIfNotWholeRoom = 10f;

		private const int MaxRoomCellsCountToUseWholeRoom = 324;

		public static bool AcceptableMapConditionsToStartParty()
		{
			if (!PartyUtility.AcceptableMapConditionsToContinueParty())
			{
				return false;
			}
			if (GenDate.HourInt < 4 || GenDate.HourInt > 21)
			{
				return false;
			}
			if (Find.StoryWatcher.watcherDanger.DangerRating != StoryDanger.None)
			{
				return false;
			}
			int freeColonistsSpawnedCount = Find.MapPawns.FreeColonistsSpawnedCount;
			if (freeColonistsSpawnedCount < 4)
			{
				return false;
			}
			int num = 0;
			foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
			{
				if (current.health.hediffSet.BleedingRate > 0f)
				{
					bool result = false;
					return result;
				}
				if (current.Drafted)
				{
					num++;
				}
			}
			if ((float)num / (float)freeColonistsSpawnedCount >= 0.5f)
			{
				return false;
			}
			int num2 = Mathf.RoundToInt((float)Find.MapPawns.FreeColonistsSpawnedCount * 0.65f);
			num2 = Mathf.Clamp(num2, 2, 10);
			int num3 = 0;
			foreach (Pawn current2 in Find.MapPawns.FreeColonistsSpawned)
			{
				if (PartyUtility.ShouldPawnKeepPartying(current2))
				{
					num3++;
				}
			}
			return num3 >= num2;
		}

		public static bool AcceptableMapConditionsToContinueParty()
		{
			return Find.StoryWatcher.watcherDanger.DangerRating != StoryDanger.High;
		}

		public static Pawn FindRandomPartyOrganizer(Faction faction)
		{
			Predicate<Pawn> validator = (Pawn x) => x.RaceProps.Humanlike && !x.InBed() && PartyUtility.ShouldPawnKeepPartying(x);
			Pawn result;
			if ((from x in Find.MapPawns.SpawnedPawnsInFaction(faction)
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

		public static bool InPartyArea(IntVec3 cell, IntVec3 partySpot)
		{
			if (PartyUtility.UseWholeRoomAsPartyArea(partySpot) && cell.GetRoom() == partySpot.GetRoom())
			{
				return true;
			}
			if (!cell.InHorDistOf(partySpot, 10f))
			{
				return false;
			}
			Building edifice = cell.GetEdifice();
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.None, false);
			if (edifice != null)
			{
				return partySpot.CanReach(edifice, PathEndMode.ClosestTouch, traverseParams);
			}
			return partySpot.CanReach(cell, PathEndMode.ClosestTouch, traverseParams);
		}

		public static bool TryFindRandomCellInPartyArea(Pawn pawn, out IntVec3 result)
		{
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			Predicate<IntVec3> validator = (IntVec3 x) => x.Standable() && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1);
			if (PartyUtility.UseWholeRoomAsPartyArea(cell))
			{
				Room room = cell.GetRoom();
				return (from x in room.Cells
				where validator(x)
				select x).TryRandomElement(out result);
			}
			return CellFinder.TryFindRandomReachableCellNear(cell, 10f, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => validator(x), null, out result, 10);
		}

		public static bool UseWholeRoomAsPartyArea(IntVec3 partySpot)
		{
			Room room = partySpot.GetRoom();
			return room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount <= 324;
		}
	}
}
