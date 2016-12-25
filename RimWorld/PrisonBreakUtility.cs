using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class PrisonBreakUtility
	{
		private const float BaseInitiatePrisonBreakMtbDays = 48f;

		private const float DistanceToJoinPrisonBreak = 20f;

		private const float ChanceForRoomToJoinPrisonBreak = 0.5f;

		private const float SapperChance = 0.5f;

		private static HashSet<Room> participatingRooms = new HashSet<Room>();

		private static List<Pawn> allEscapingPrisoners = new List<Pawn>();

		private static List<Room> tmpToRemove = new List<Room>();

		private static List<Pawn> escapingPrisonersGroup = new List<Pawn>();

		public static float InitiatePrisonBreakMtbDays(Pawn pawn)
		{
			if (!pawn.Awake())
			{
				return -1f;
			}
			if (!PrisonBreakUtility.CanParticipateInPrisonBreak(pawn))
			{
				return -1f;
			}
			Room room = pawn.GetRoom();
			if (room == null || !room.isPrisonCell)
			{
				return -1f;
			}
			float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
			float num = Mathf.Lerp(2.3f, 0.8f, GenMath.GetFactorInInterval(12f, 18f, 65f, 0.67f, ageBiologicalYearsFloat));
			float num2 = Mathf.Lerp(3f, 1f, pawn.health.summaryHealth.SummaryHealthPercent);
			float num3 = Mathf.Lerp(0.7f, 2f, pawn.needs.mood.CurLevelPercentage);
			return 48f * num * num2 * num3;
		}

		public static bool CanParticipateInPrisonBreak(Pawn pawn)
		{
			return !pawn.Downed && pawn.IsPrisoner && !PrisonBreakUtility.IsPrisonBreaking(pawn);
		}

		public static bool IsPrisonBreaking(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			return lord != null && lord.LordJob is LordJob_PrisonBreak;
		}

		public static void StartPrisonBreak(Pawn initiator)
		{
			PrisonBreakUtility.participatingRooms.Clear();
			foreach (IntVec3 current in GenRadial.RadialCellsAround(initiator.Position, 20f, true))
			{
				if (current.InBounds())
				{
					Room room = current.GetRoom();
					if (room != null && room.isPrisonCell)
					{
						PrisonBreakUtility.participatingRooms.Add(room);
					}
				}
			}
			PrisonBreakUtility.RemoveRandomRooms(PrisonBreakUtility.participatingRooms, initiator);
			int sapperThingID = -1;
			if (Rand.Value < 0.5f)
			{
				sapperThingID = initiator.thingIDNumber;
			}
			PrisonBreakUtility.allEscapingPrisoners.Clear();
			foreach (Room current2 in PrisonBreakUtility.participatingRooms)
			{
				PrisonBreakUtility.StartPrisonBreakIn(current2, PrisonBreakUtility.allEscapingPrisoners, sapperThingID, PrisonBreakUtility.participatingRooms);
			}
			PrisonBreakUtility.participatingRooms.Clear();
			if (PrisonBreakUtility.allEscapingPrisoners.Any<Pawn>())
			{
				PrisonBreakUtility.SendPrisonBreakLetter(PrisonBreakUtility.allEscapingPrisoners);
			}
			PrisonBreakUtility.allEscapingPrisoners.Clear();
		}

		private static void RemoveRandomRooms(HashSet<Room> participatingRooms, Pawn initiator)
		{
			Room room = initiator.GetRoom();
			PrisonBreakUtility.tmpToRemove.Clear();
			foreach (Room current in participatingRooms)
			{
				if (current != room)
				{
					if (Rand.Value >= 0.5f)
					{
						PrisonBreakUtility.tmpToRemove.Add(current);
					}
				}
			}
			for (int i = 0; i < PrisonBreakUtility.tmpToRemove.Count; i++)
			{
				participatingRooms.Remove(PrisonBreakUtility.tmpToRemove[i]);
			}
			PrisonBreakUtility.tmpToRemove.Clear();
		}

		private static void StartPrisonBreakIn(Room room, List<Pawn> outAllEscapingPrisoners, int sapperThingID, HashSet<Room> participatingRooms)
		{
			PrisonBreakUtility.escapingPrisonersGroup.Clear();
			PrisonBreakUtility.AddPrisonersFrom(room, PrisonBreakUtility.escapingPrisonersGroup);
			if (!PrisonBreakUtility.escapingPrisonersGroup.Any<Pawn>())
			{
				return;
			}
			foreach (Room current in participatingRooms)
			{
				if (current != room)
				{
					if (PrisonBreakUtility.RoomsAreCloseToEachOther(room, current))
					{
						PrisonBreakUtility.AddPrisonersFrom(current, PrisonBreakUtility.escapingPrisonersGroup);
					}
				}
			}
			IntVec3 exitPoint;
			if (!RCellFinder.TryFindRandomExitSpot(PrisonBreakUtility.escapingPrisonersGroup[0], out exitPoint, TraverseMode.PassDoors))
			{
				return;
			}
			IntVec3 groupUpLoc;
			if (!PrisonBreakUtility.TryFindGroupUpLoc(PrisonBreakUtility.escapingPrisonersGroup, exitPoint, out groupUpLoc))
			{
				return;
			}
			LordMaker.MakeNewLord(PrisonBreakUtility.escapingPrisonersGroup[0].Faction, new LordJob_PrisonBreak(groupUpLoc, exitPoint, sapperThingID), PrisonBreakUtility.escapingPrisonersGroup);
			for (int i = 0; i < PrisonBreakUtility.escapingPrisonersGroup.Count; i++)
			{
				Pawn pawn = PrisonBreakUtility.escapingPrisonersGroup[i];
				if (pawn.CurJob != null && pawn.jobs.curDriver.layingDown)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				else
				{
					pawn.jobs.CheckForJobOverride();
				}
				outAllEscapingPrisoners.Add(pawn);
			}
			PrisonBreakUtility.escapingPrisonersGroup.Clear();
		}

		private static void AddPrisonersFrom(Room room, List<Pawn> outEscapingPrisoners)
		{
			foreach (Thing current in room.AllContainedThings)
			{
				Pawn pawn = current as Pawn;
				if (pawn != null && PrisonBreakUtility.CanParticipateInPrisonBreak(pawn))
				{
					outEscapingPrisoners.Add(pawn);
				}
			}
		}

		private static bool TryFindGroupUpLoc(List<Pawn> escapingPrisoners, IntVec3 exitPoint, out IntVec3 groupUpLoc)
		{
			groupUpLoc = IntVec3.Invalid;
			using (PawnPath pawnPath = PathFinder.FindPath(escapingPrisoners[0].Position, exitPoint, TraverseParms.For(escapingPrisoners[0], Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
			{
				if (!pawnPath.Found)
				{
					Log.Warning("Prison break: could not find path for prisoner " + escapingPrisoners[0] + " to the exit point.");
					return false;
				}
				for (int i = 0; i < pawnPath.NodesLeftCount; i++)
				{
					IntVec3 intVec = pawnPath.Peek(pawnPath.NodesLeftCount - i - 1);
					Room room = intVec.GetRoom();
					if (room != null)
					{
						if (!room.isPrisonCell)
						{
							if (!room.TouchesMapEdge && !room.IsHuge)
							{
								if (room.Cells.Count((IntVec3 x) => x.Standable()) < 5)
								{
									goto IL_F1;
								}
							}
							groupUpLoc = CellFinder.RandomClosewalkCellNear(intVec, 3);
						}
					}
					IL_F1:;
				}
			}
			if (!groupUpLoc.IsValid)
			{
				groupUpLoc = escapingPrisoners[0].Position;
			}
			return true;
		}

		private static bool RoomsAreCloseToEachOther(Room a, Room b)
		{
			IntVec3 intVec = a.Cells.RandomElement<IntVec3>();
			IntVec3 intVec2 = b.Cells.RandomElement<IntVec3>();
			if (!intVec.WithinRegions(intVec2, 18, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
			{
				return false;
			}
			bool result;
			using (PawnPath pawnPath = PathFinder.FindPath(intVec, intVec2, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false), PathEndMode.OnCell))
			{
				if (!pawnPath.Found)
				{
					result = false;
				}
				else
				{
					result = (pawnPath.NodesLeftCount < 24);
				}
			}
			return result;
		}

		private static void SendPrisonBreakLetter(List<Pawn> escapingPrisoners)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < escapingPrisoners.Count; i++)
			{
				stringBuilder.AppendLine("    " + escapingPrisoners[i].LabelShort);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelPrisonBreak".Translate(), "LetterPrisonBreak".Translate(new object[]
			{
				stringBuilder.ToString().TrimEndNewlines()
			}), LetterType.BadUrgent, PrisonBreakUtility.allEscapingPrisoners[0], null);
		}
	}
}
