using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class PawnDiedOrDownedThoughtsUtility
	{
		private static List<IndividualThoughtToAdd> tmpIndividualThoughtsToAdd = new List<IndividualThoughtToAdd>();

		private static List<ThoughtDef> tmpAllColonistsThoughts = new List<ThoughtDef>();

		public static void TryGiveThoughts(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind)
		{
			try
			{
				if (!PawnGenerator.IsBeingGenerated(victim))
				{
					if (Current.ProgramState == ProgramState.Playing)
					{
						PawnDiedOrDownedThoughtsUtility.GetThoughts(victim, dinfo, thoughtsKind, PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd, PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts);
						for (int i = 0; i < PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd.Count; i++)
						{
							PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd[i].Add();
						}
						if (PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Any<ThoughtDef>())
						{
							foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
							{
								if (current != victim)
								{
									for (int j = 0; j < PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Count; j++)
									{
										ThoughtDef def = PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts[j];
										current.needs.mood.thoughts.memories.TryGainMemory(def, null);
									}
								}
							}
						}
						PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd.Clear();
						PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Clear();
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Could not give thoughts: " + arg, false);
			}
		}

		public static void TryGiveThoughts(IEnumerable<Pawn> victims, PawnDiedOrDownedThoughtsKind thoughtsKind)
		{
			foreach (Pawn current in victims)
			{
				PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(current, null, thoughtsKind);
			}
		}

		public static void GetThoughts(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			outIndividualThoughts.Clear();
			outAllColonistsThoughts.Clear();
			if (victim.RaceProps.Humanlike)
			{
				PawnDiedOrDownedThoughtsUtility.AppendThoughts_ForHumanlike(victim, dinfo, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
			}
			if (victim.relations != null && victim.relations.everSeenByPlayer)
			{
				PawnDiedOrDownedThoughtsUtility.AppendThoughts_Relations(victim, dinfo, thoughtsKind, outIndividualThoughts, outAllColonistsThoughts);
			}
		}

		public static void BuildMoodThoughtsListString(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, StringBuilder sb, string individualThoughtsHeader, string allColonistsThoughtsHeader)
		{
			PawnDiedOrDownedThoughtsUtility.GetThoughts(victim, dinfo, thoughtsKind, PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd, PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts);
			if (PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Any<ThoughtDef>())
			{
				if (!allColonistsThoughtsHeader.NullOrEmpty())
				{
					sb.Append(allColonistsThoughtsHeader);
					sb.AppendLine();
				}
				for (int i = 0; i < PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Count; i++)
				{
					ThoughtDef thoughtDef = PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts[i];
					if (sb.Length > 0)
					{
						sb.AppendLine();
					}
					sb.Append("  - " + thoughtDef.stages[0].label.CapitalizeFirst() + " " + Mathf.RoundToInt(thoughtDef.stages[0].baseMoodEffect).ToStringWithSign());
				}
			}
			if (PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd.Any((IndividualThoughtToAdd x) => x.thought.MoodOffset() != 0f))
			{
				if (!individualThoughtsHeader.NullOrEmpty())
				{
					sb.Append(individualThoughtsHeader);
				}
				foreach (IGrouping<Pawn, IndividualThoughtToAdd> current in from x in PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd
				where x.thought.MoodOffset() != 0f
				group x by x.addTo)
				{
					if (sb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine();
					}
					string value = current.Key.KindLabel.CapitalizeFirst() + " " + current.Key.LabelShort;
					sb.Append(value);
					sb.Append(":");
					foreach (IndividualThoughtToAdd current2 in current)
					{
						sb.AppendLine();
						sb.Append("    " + current2.LabelCap);
					}
				}
			}
		}

		public static void BuildMoodThoughtsListString(IEnumerable<Pawn> victims, PawnDiedOrDownedThoughtsKind thoughtsKind, StringBuilder sb, string individualThoughtsHeader, string allColonistsThoughtsHeader, string victimLabelKey)
		{
			foreach (Pawn current in victims)
			{
				PawnDiedOrDownedThoughtsUtility.GetThoughts(current, null, thoughtsKind, PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd, PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts);
				if (PawnDiedOrDownedThoughtsUtility.tmpIndividualThoughtsToAdd.Any<IndividualThoughtToAdd>() || PawnDiedOrDownedThoughtsUtility.tmpAllColonistsThoughts.Any<ThoughtDef>())
				{
					if (sb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine();
					}
					string text = current.KindLabel.CapitalizeFirst() + " " + current.LabelShort;
					if (victimLabelKey.NullOrEmpty())
					{
						sb.Append(text + ":");
					}
					else
					{
						sb.Append(victimLabelKey.Translate(text));
					}
					PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(current, null, thoughtsKind, sb, individualThoughtsHeader, allColonistsThoughtsHeader);
				}
			}
		}

		private static void AppendThoughts_ForHumanlike(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			bool flag = dinfo.HasValue && dinfo.Value.Def.execution;
			bool flag2 = victim.IsPrisonerOfColony && !victim.guilt.IsGuilty && !victim.InAggroMentalState;
			bool flag3 = dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(victim) && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn;
			if (flag3)
			{
				Pawn pawn = (Pawn)dinfo.Value.Instigator;
				if (!pawn.Dead && pawn.needs.mood != null && pawn.story != null && pawn != victim)
				{
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeBloodlust, pawn, null, 1f, 1f));
					}
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.HostileTo(pawn) && victim.Faction != null && PawnUtility.IsFactionLeader(victim) && victim.Faction.HostileTo(pawn.Faction))
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedHostileFactionLeader, pawn, victim, 1f, 1f));
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && !flag)
			{
				foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (current != victim && current.needs != null && current.needs.mood != null)
					{
						if (current.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)current.MentalState).otherPawn != victim)
						{
							if (PawnDiedOrDownedThoughtsUtility.Witnessed(current, victim))
							{
								if (current.Faction == victim.Faction)
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathAlly, current, null, 1f, 1f));
								}
								else if (victim.Faction == null || !victim.Faction.HostileTo(current.Faction))
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathNonAlly, current, null, 1f, 1f));
								}
								if (current.relations.FamilyByBlood.Contains(victim))
								{
									outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathFamily, current, null, 1f, 1f));
								}
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.WitnessedDeathBloodlust, current, null, 1f, 1f));
							}
							else if (victim.Faction == Faction.OfPlayer && victim.Faction == current.Faction && victim.HostFaction != current.Faction)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowColonistDied, current, victim, 1f, 1f));
							}
							if (flag2 && current.Faction == Faction.OfPlayer && !current.IsPrisoner)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KnowPrisonerDiedInnocent, current, victim, 1f, 1f));
							}
						}
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Banished && victim.IsColonist)
			{
				outAllColonistsThoughts.Add(ThoughtDefOf.ColonistBanished);
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.BanishedToDie)
			{
				if (victim.IsColonist)
				{
					outAllColonistsThoughts.Add(ThoughtDefOf.ColonistBanishedToDie);
				}
				else if (victim.IsPrisonerOfColony)
				{
					outAllColonistsThoughts.Add(ThoughtDefOf.PrisonerBanishedToDie);
				}
			}
		}

		private static void AppendThoughts_Relations(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
		{
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Banished && victim.RaceProps.Animal)
			{
				List<DirectPawnRelation> directRelations = victim.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (directRelations[i].otherPawn.needs != null && directRelations[i].otherPawn.needs.mood != null)
					{
						if (PawnUtility.ShouldGetThoughtAbout(directRelations[i].otherPawn, victim))
						{
							if (directRelations[i].def == PawnRelationDefOf.Bond)
							{
								outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.BondedAnimalBanished, directRelations[i].otherPawn, victim, 1f, 1f));
							}
						}
					}
				}
			}
			if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died || thoughtsKind == PawnDiedOrDownedThoughtsKind.BanishedToDie)
			{
				foreach (Pawn current in victim.relations.PotentiallyRelatedPawns)
				{
					if (current.needs != null && current.needs.mood != null)
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
							if (pawn != current2 && current2.needs != null && current2.needs.mood != null)
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
										ThoughtDef thoughtDef = ThoughtDefOf.KilledMyFriend;
										Pawn pawn2 = current2;
										Pawn pawn3 = pawn;
										float opinionOffsetFactor = victim.relations.GetFriendDiedThoughtPowerFactor(num);
										outIndividualThoughts.Add(new IndividualThoughtToAdd(thoughtDef, pawn2, pawn3, 1f, opinionOffsetFactor));
									}
									else if (num <= -20)
									{
										ThoughtDef thoughtDef = ThoughtDefOf.KilledMyRival;
										Pawn pawn3 = current2;
										Pawn pawn2 = pawn;
										float opinionOffsetFactor = victim.relations.GetRivalDiedThoughtPowerFactor(num);
										outIndividualThoughts.Add(new IndividualThoughtToAdd(thoughtDef, pawn3, pawn2, 1f, opinionOffsetFactor));
									}
								}
							}
						}
					}
				}
				if (victim.RaceProps.Humanlike)
				{
					foreach (Pawn current3 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
					{
						if (current3.needs != null && current3.RaceProps.IsFlesh && current3.needs.mood != null)
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

		private static bool Witnessed(Pawn p, Pawn victim)
		{
			if (!p.Awake() || !p.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return false;
			}
			if (victim.IsCaravanMember())
			{
				return victim.GetCaravan() == p.GetCaravan();
			}
			return victim.Spawned && p.Spawned && p.Position.InHorDistOf(victim.Position, 12f) && GenSight.LineOfSight(victim.Position, p.Position, victim.Map, false, null, 0, 0);
		}

		public static void RemoveDiedThoughts(Pawn pawn)
		{
			foreach (Pawn current in PawnsFinder.AllMapsWorldAndTemporary_Alive)
			{
				if (current.needs != null && current.needs.mood != null && current != pawn)
				{
					MemoryThoughtHandler memories = current.needs.mood.thoughts.memories;
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.KnowColonistDied, pawn);
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.KnowPrisonerDiedInnocent, pawn);
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithGoodOpinionDied, pawn);
					memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.PawnWithBadOpinionDied, pawn);
					List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						ThoughtDef genderSpecificDiedThought = allDefsListForReading[i].GetGenderSpecificDiedThought(pawn);
						if (genderSpecificDiedThought != null)
						{
							memories.RemoveMemoriesOfDefWhereOtherPawnIs(genderSpecificDiedThought, pawn);
						}
					}
				}
			}
		}
	}
}
