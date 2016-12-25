using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class PawnDiedOrDownedThoughtsUtility
	{
		private static List<IndividualThoughtToAdd> tmpIndividualThoughtsToAdd = new List<IndividualThoughtToAdd>();

		private static List<ThoughtDef> tmpAllColonistsThoughts = new List<ThoughtDef>();

		public static void TryGiveThoughts(Pawn victim, DamageInfo? dinfo, Hediff hediff, PawnDiedOrDownedThoughtsKind thoughtsKind)
		{
			try
			{
				if (!PawnGenerator.IsBeingGenerated(victim))
				{
					if (Current.ProgramState == ProgramState.Playing)
					{
						PawnDiedOrDownedThoughtsUtility.GetThoughts(victim, dinfo, hediff, thoughtsKind, PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd, PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts);
						for (int i = 0; i < PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd.Count; i++)
						{
							PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd[i].Add();
						}
						if (PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Any<ThoughtDef>())
						{
							foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists)
							{
								for (int j = 0; j < PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Count; j++)
								{
									ThoughtDef def = PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts[j];
									current.needs.mood.thoughts.memories.TryGainMemoryThought(def, null);
								}
							}
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Could not give thoughts: " + arg);
			}
		}

		public static void GetThoughts(Pawn victim, DamageInfo? dinfo, Hediff hediff, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			outIndividualThoughts.Clear();
			outAllColonistsThoughts.Clear();
			if (victim.RaceProps.Humanlike)
			{
				PawnDiedOrDownedThoughtsUtility.AppendThoughts_Humanlike(victim, dinfo, hediff, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
			}
			if (victim.relations != null && victim.relations.everSeenByPlayer)
			{
				PawnDiedOrDownedThoughtsUtility.AppendThoughts_Relations(victim, dinfo, hediff, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
			}
		}

		private static bool IsExecution(DamageInfo? dinfo, Hediff hediff)
		{
			return (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.ExecutionCut) || (hediff != null && (hediff.def == HediffDefOf.Euthanasia || hediff.def == HediffDefOf.ShutDown));
		}

		private static bool IsInnocentPrisoner(Pawn pawn)
		{
			return pawn.IsPrisonerOfColony && !pawn.guilt.IsGuilty && !pawn.InAggroMentalState;
		}

		private static void AppendThoughts_Humanlike(Pawn victim, DamageInfo? dinfo, Hediff hediff, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			bool flag = PawnDiedOrDownedThoughtsUtility.IsExecution(dinfo, hediff);
			bool flag2 = PawnDiedOrDownedThoughtsUtility.IsInnocentPrisoner(victim);
			bool flag3 = dinfo.HasValue && dinfo.Value.Def.externalViolence && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn;
			if (flag3)
			{
				Pawn pawn = (Pawn)dinfo.Value.Instigator;
				if (!pawn.Dead && pawn.needs.mood != null && pawn.story != null && pawn != victim)
				{
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeBloodlust, pawn, null, 1f, 1f));
					}
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.HostileTo(pawn))
					{
						if (victim.Faction != null && PawnUtility.IsFactionLeader(victim) && victim.Faction.HostileTo(pawn.Faction))
						{
							outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedHostileFactionLeader, pawn, victim, 1f, 1f));
						}
						if (victim.kindDef.combatPower > 250f)
						{
							outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedMajorEnemy, pawn, victim, 1f, 1f));
						}
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.Spawned)
			{
				List<Pawn> allPawnsSpawned = victim.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn2 = allPawnsSpawned[i];
					if (pawn2 != victim && pawn2.needs.mood != null)
					{
						if (!flag && (pawn2.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)pawn2.MentalState).otherPawn != victim))
						{
							if (pawn2.Position.InHorDistOf(victim.Position, 12f) && GenSight.LineOfSight(victim.Position, pawn2.Position, victim.Map, false) && pawn2.Awake() && pawn2.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
							{
								if (pawn2.Faction == victim.Faction)
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathAlly, pawn2, null, 1f, 1f));
								}
								else if (victim.Faction == null || !victim.Faction.HostileTo(pawn2.Faction))
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathNonAlly, pawn2, null, 1f, 1f));
								}
								if (pawn2.relations.FamilyByBlood.Contains(victim))
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathFamily, pawn2, null, 1f, 1f));
								}
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathBloodlust, pawn2, null, 1f, 1f));
							}
							else if (victim.Faction == Faction.OfPlayer && victim.Faction == pawn2.Faction && victim.HostFaction != pawn2.Faction)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowColonistDied, pawn2, null, 1f, 1f));
							}
							if (flag2 && pawn2.Faction == Faction.OfPlayer && !pawn2.IsPrisoner)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowPrisonerDiedInnocent, pawn2, null, 1f, 1f));
							}
						}
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Abandoned && victim.IsColonist)
			{
				outAllColonistsThoughts.Add(ThoughtDefOf.ColonistAbandoned);
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.AbandonedToDie)
			{
				if (victim.IsColonist)
				{
					outAllColonistsThoughts.Add(ThoughtDefOf.ColonistAbandonedToDie);
				}
				else if (victim.IsPrisonerOfColony)
				{
					outAllColonistsThoughts.Add(ThoughtDefOf.PrisonerAbandonedToDie);
				}
			}
		}

		private static void AppendThoughts_Relations(Pawn victim, DamageInfo? dinfo, Hediff hediff, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Abandoned && victim.RaceProps.Animal)
			{
				List<DirectPawnRelation> directRelations = victim.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (!directRelations[i].otherPawn.Dead && directRelations[i].otherPawn.needs.mood != null)
					{
						if (PawnUtility.ShouldGetThoughtAbout(directRelations[i].otherPawn, victim))
						{
							if (directRelations[i].def == PawnRelationDefOf.Bond)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.BondedAnimalAbandoned, directRelations[i].otherPawn, victim, 1f, 1f));
							}
						}
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died || thoughtsKind == PawnDiedOrDownedThoughtsKind.AbandonedToDie)
			{
				foreach (Pawn current in victim.relations.PotentiallyRelatedPawns)
				{
					if (!current.Dead && current.needs.mood != null)
					{
						if (PawnUtility.ShouldGetThoughtAbout(current, victim))
						{
							PawnRelationDef mostImportantRelation = current.GetMostImportantRelation(victim);
							if (mostImportantRelation != null)
							{
								ThoughtDef genderSpecificDiedThought = mostImportantRelation.GetGenderSpecificDiedThought(victim);
								if (genderSpecificDiedThought != null)
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(genderSpecificDiedThought, current, victim, 1f, 1f));
								}
							}
						}
					}
				}
				if (dinfo.HasValue)
				{
					Pawn pawn = dinfo.Value.Instigator as Pawn;
					if (pawn != null && pawn != victim)
					{
						foreach (Pawn current2 in victim.relations.PotentiallyRelatedPawns)
						{
							if (pawn != current2 && !current2.Dead && current2.needs.mood != null)
							{
								PawnRelationDef mostImportantRelation2 = current2.GetMostImportantRelation(victim);
								if (mostImportantRelation2 != null)
								{
									ThoughtDef genderSpecificKilledThought = mostImportantRelation2.GetGenderSpecificKilledThought(victim);
									if (genderSpecificKilledThought != null)
									{
										outIndividualThoughts.Add(new IndividualThoughtToAdd(genderSpecificKilledThought, current2, pawn, 1f, 1f));
									}
								}
								if (current2.RaceProps.IsFlesh)
								{
									int num = current2.relations.OpinionOf(victim);
									if (num >= 20)
									{
										float opinionOffsetFactor = victim.relations.GetFriendDiedThoughtPowerFactor(num);
										outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledMyFriend, current2, pawn, 1f, opinionOffsetFactor));
									}
									else if (num <= -20)
									{
										float opinionOffsetFactor = victim.relations.GetRivalDiedThoughtPowerFactor(num);
										outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledMyRival, current2, pawn, 1f, opinionOffsetFactor));
									}
								}
							}
						}
					}
				}
				if (victim.RaceProps.Humanlike)
				{
					foreach (Pawn current3 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods)
					{
						if (!current3.Dead && current3.RaceProps.IsFlesh && current3.needs.mood != null)
						{
							if (PawnUtility.ShouldGetThoughtAbout(current3, victim))
							{
								int num2 = current3.relations.OpinionOf(victim);
								if (num2 >= 20)
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.PawnWithGoodOpinionDied, current3, victim, victim.relations.GetFriendDiedThoughtPowerFactor(num2), 1f));
								}
								else if (num2 <= -20)
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.PawnWithBadOpinionDied, current3, victim, victim.relations.GetRivalDiedThoughtPowerFactor(num2), 1f));
								}
							}
						}
					}
				}
			}
		}
	}
}
