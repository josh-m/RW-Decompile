using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class MarriageCeremonyUtility
	{
		public static bool AcceptableMapConditionsToStartCeremony(Map map)
		{
			if (!MarriageCeremonyUtility.AcceptableMapConditionsToContinueCeremony(map))
			{
				return false;
			}
			if (GenLocalDate.HourInt(map) < 5 || GenLocalDate.HourInt(map) > 16)
			{
				return false;
			}
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				if (lords[i].LordJob is LordJob_Joinable_Party || lords[i].LordJob is LordJob_Joinable_MarriageCeremony)
				{
					return false;
				}
			}
			if (map.dangerWatcher.DangerRating != StoryDanger.None)
			{
				return false;
			}
			int num = 0;
			foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
			{
				if (current.Drafted)
				{
					num++;
				}
			}
			return (float)num / (float)map.mapPawns.FreeColonistsSpawnedCount < 0.5f;
		}

		public static bool AcceptableMapConditionsToContinueCeremony(Map map)
		{
			return map.dangerWatcher.DangerRating != StoryDanger.High;
		}

		public static bool FianceReadyToStartCeremony(Pawn pawn)
		{
			return MarriageCeremonyUtility.FianceCanContinueCeremony(pawn) && pawn.health.hediffSet.BleedRateTotal <= 0f && !HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) && !PawnUtility.WillSoonHaveBasicNeed(pawn) && !MarriageCeremonyUtility.IsCurrentlyMarryingSomeone(pawn) && (!pawn.Drafted && !pawn.InMentalState && pawn.Awake() && !pawn.IsBurning()) && !pawn.InBed();
		}

		public static bool FianceCanContinueCeremony(Pawn pawn)
		{
			if (pawn.health.hediffSet.BleedRateTotal > 0.3f)
			{
				return false;
			}
			if (pawn.IsPrisoner)
			{
				return false;
			}
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			return (firstHediffOfDef == null || firstHediffOfDef.Severity <= 0.2f) && (pawn.Spawned && !pawn.Downed) && !pawn.InAggroMentalState;
		}

		public static bool ShouldGuestKeepAttendingCeremony(Pawn p)
		{
			return GatheringsUtility.ShouldGuestKeepAttendingGathering(p);
		}

		public static void Married(Pawn firstPawn, Pawn secondPawn)
		{
			LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse(firstPawn);
			LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse(secondPawn);
			firstPawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, secondPawn);
			firstPawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExSpouse, secondPawn);
			firstPawn.relations.AddDirectRelation(PawnRelationDefOf.Spouse, secondPawn);
			MarriageCeremonyUtility.AddNewlyMarriedThoughts(firstPawn, secondPawn);
			MarriageCeremonyUtility.AddNewlyMarriedThoughts(secondPawn, firstPawn);
			firstPawn.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.DivorcedMe, secondPawn);
			secondPawn.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.DivorcedMe, firstPawn);
			LovePartnerRelationUtility.TryToShareBed(firstPawn, secondPawn);
			TaleRecorder.RecordTale(TaleDefOf.Marriage, new object[]
			{
				firstPawn,
				secondPawn
			});
		}

		private static void AddNewlyMarriedThoughts(Pawn pawn, Pawn otherPawn)
		{
			pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.GotMarried, otherPawn);
			pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.HoneymoonPhase, otherPawn);
		}

		private static bool IsCurrentlyMarryingSomeone(Pawn p)
		{
			if (!p.Spawned)
			{
				return false;
			}
			List<Lord> lords = p.Map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				LordJob_Joinable_MarriageCeremony lordJob_Joinable_MarriageCeremony = lords[i].LordJob as LordJob_Joinable_MarriageCeremony;
				if (lordJob_Joinable_MarriageCeremony != null && (lordJob_Joinable_MarriageCeremony.firstPawn == p || lordJob_Joinable_MarriageCeremony.secondPawn == p))
				{
					return true;
				}
			}
			return false;
		}
	}
}
