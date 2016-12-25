using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_RelationsTracker : IExposable
	{
		private const int CheckDevelopBondRelationIntervalTicks = 2500;

		private const float MaxBondRelationCheckDist = 12f;

		private const float BondRelationPerIntervalChance = 0.01f;

		private const int FriendOpinionThreshold = 20;

		private const int RivalOpinionThreshold = -20;

		private Pawn pawn;

		private List<DirectPawnRelation> directRelations = new List<DirectPawnRelation>();

		public bool everSeenByPlayer;

		public bool canGetRescuedThought = true;

		private HashSet<Pawn> pawnsWithDirectRelationsWithMe = new HashSet<Pawn>();

		public List<DirectPawnRelation> DirectRelations
		{
			get
			{
				return this.directRelations;
			}
		}

		public IEnumerable<Pawn> Children
		{
			get
			{
				foreach (Pawn p in this.pawnsWithDirectRelationsWithMe)
				{
					if (p.relations.directRelations.Find((DirectPawnRelation x) => x.otherPawn == this.<>f__this.pawn && x.def == PawnRelationDefOf.Parent) != null)
					{
						yield return p;
					}
				}
			}
		}

		public int ChildrenCount
		{
			get
			{
				return this.Children.Count<Pawn>();
			}
		}

		public bool RelatedToAnyoneOrAnyoneRelatedToMe
		{
			get
			{
				return this.directRelations.Any<DirectPawnRelation>() || this.pawnsWithDirectRelationsWithMe.Any<Pawn>();
			}
		}

		public IEnumerable<Pawn> FamilyByBlood
		{
			get
			{
				if (this.RelatedToAnyoneOrAnyoneRelatedToMe)
				{
					Stack<Pawn> familyStack = null;
					Stack<Pawn> familyChildrenStack = null;
					HashSet<Pawn> familyVisited = null;
					try
					{
						familyStack = SimplePool<Stack<Pawn>>.Get();
						familyChildrenStack = SimplePool<Stack<Pawn>>.Get();
						familyVisited = SimplePool<HashSet<Pawn>>.Get();
						familyStack.Push(this.pawn);
						familyVisited.Add(this.pawn);
						while (familyStack.Any<Pawn>())
						{
							Pawn p = familyStack.Pop();
							if (p != this.pawn)
							{
								yield return p;
							}
							Pawn father = p.GetFather();
							if (father != null && !familyVisited.Contains(father))
							{
								familyStack.Push(father);
								familyVisited.Add(father);
							}
							Pawn mother = p.GetMother();
							if (mother != null && !familyVisited.Contains(mother))
							{
								familyStack.Push(mother);
								familyVisited.Add(mother);
							}
							familyChildrenStack.Clear();
							familyChildrenStack.Push(p);
							while (familyChildrenStack.Any<Pawn>())
							{
								Pawn child = familyChildrenStack.Pop();
								if (child != p && child != this.pawn)
								{
									yield return child;
								}
								IEnumerable<Pawn> children = child.relations.Children;
								foreach (Pawn c in children)
								{
									if (!familyVisited.Contains(c))
									{
										familyChildrenStack.Push(c);
										familyVisited.Add(c);
									}
								}
							}
						}
					}
					finally
					{
						base.<>__Finally0();
					}
				}
			}
		}

		public IEnumerable<Pawn> PotentiallyRelatedPawns
		{
			get
			{
				if (this.RelatedToAnyoneOrAnyoneRelatedToMe)
				{
					Stack<Pawn> stack = null;
					HashSet<Pawn> visited = null;
					try
					{
						stack = SimplePool<Stack<Pawn>>.Get();
						visited = SimplePool<HashSet<Pawn>>.Get();
						stack.Push(this.pawn);
						visited.Add(this.pawn);
						while (stack.Any<Pawn>())
						{
							Pawn p = stack.Pop();
							if (p != this.pawn)
							{
								yield return p;
							}
							for (int i = 0; i < p.relations.directRelations.Count; i++)
							{
								Pawn other = p.relations.directRelations[i].otherPawn;
								if (!visited.Contains(other))
								{
									stack.Push(other);
									visited.Add(other);
								}
							}
							foreach (Pawn other2 in p.relations.pawnsWithDirectRelationsWithMe)
							{
								if (!visited.Contains(other2))
								{
									stack.Push(other2);
									visited.Add(other2);
								}
							}
						}
					}
					finally
					{
						base.<>__Finally0();
					}
				}
			}
		}

		public IEnumerable<Pawn> RelatedPawns
		{
			get
			{
				return from x in this.PotentiallyRelatedPawns
				where this.pawn.GetRelations(x).Any<PawnRelationDef>()
				select x;
			}
		}

		public Pawn_RelationsTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<DirectPawnRelation>(ref this.directRelations, "directRelations", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					if (this.directRelations[i].otherPawn == null)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Pawn ",
							this.pawn,
							" has relation \"",
							this.directRelations[i].def.defName,
							"\" with null pawn after loading. This means that we forgot to serialize pawns somewhere (e.g. pawns from passing trade ships)."
						}));
					}
				}
				this.directRelations.RemoveAll((DirectPawnRelation x) => x.otherPawn == null);
				for (int j = 0; j < this.directRelations.Count; j++)
				{
					this.directRelations[j].otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(this.pawn);
				}
			}
			Scribe_Values.LookValue<bool>(ref this.everSeenByPlayer, "everSeenByPlayer", true, false);
			Scribe_Values.LookValue<bool>(ref this.canGetRescuedThought, "canGetRescuedThought", true, false);
		}

		public void SocialTrackerTick()
		{
			if (this.pawn.Dead)
			{
				return;
			}
			this.Tick_CheckStartMarriageCeremony();
			this.Tick_CheckDevelopBondRelation();
		}

		public DirectPawnRelation GetDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.");
				return null;
			}
			return this.directRelations.Find((DirectPawnRelation x) => x.def == def && x.otherPawn == otherPawn);
		}

		public Pawn GetFirstDirectRelationPawn(PawnRelationDef def, Predicate<Pawn> predicate = null)
		{
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.");
				return null;
			}
			for (int i = 0; i < this.directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = this.directRelations[i];
				if (directPawnRelation.def == def && (predicate == null || predicate(directPawnRelation.otherPawn)))
				{
					return directPawnRelation.otherPawn;
				}
			}
			return null;
		}

		public bool DirectRelationExists(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(def + " is not a direct relation.");
				return false;
			}
			for (int i = 0; i < this.directRelations.Count; i++)
			{
				DirectPawnRelation directPawnRelation = this.directRelations[i];
				if (directPawnRelation.def == def && directPawnRelation.otherPawn == otherPawn)
				{
					return true;
				}
			}
			return false;
		}

		public void AddDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to directly add implied pawn relation ",
					def,
					", pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}));
				return;
			}
			if (otherPawn == this.pawn)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to add pawn relation ",
					def,
					" with self, pawn=",
					this.pawn
				}));
				return;
			}
			if (this.DirectRelationExists(def, otherPawn))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to add the same relation twice: ",
					def,
					", pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}));
				return;
			}
			int startTicks = (Current.ProgramState != ProgramState.MapPlaying) ? 0 : Find.TickManager.TicksGame;
			this.directRelations.Add(new DirectPawnRelation(def, otherPawn, startTicks));
			otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(this.pawn);
			if (def.reflexive)
			{
				otherPawn.relations.directRelations.Add(new DirectPawnRelation(def, this.pawn, startTicks));
				this.pawnsWithDirectRelationsWithMe.Add(otherPawn);
			}
			this.GainedOrLostDirectRelation();
			otherPawn.relations.GainedOrLostDirectRelation();
		}

		public void RemoveDirectRelation(DirectPawnRelation relation)
		{
			this.RemoveDirectRelation(relation.def, relation.otherPawn);
		}

		public void RemoveDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (!this.TryRemoveDirectRelation(def, otherPawn))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Could not remove relation ",
					def,
					" because it's not here. pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}));
			}
		}

		public bool TryRemoveDirectRelation(PawnRelationDef def, Pawn otherPawn)
		{
			if (def.implied)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to remove implied pawn relation ",
					def,
					", pawn=",
					this.pawn,
					", otherPawn=",
					otherPawn
				}));
				return false;
			}
			for (int i = 0; i < this.directRelations.Count; i++)
			{
				if (this.directRelations[i].def == def && this.directRelations[i].otherPawn == otherPawn)
				{
					if (def.reflexive)
					{
						List<DirectPawnRelation> list = otherPawn.relations.directRelations;
						DirectPawnRelation item = list.Find((DirectPawnRelation x) => x.def == def && x.otherPawn == this.pawn);
						list.Remove(item);
						if (list.Find((DirectPawnRelation x) => x.otherPawn == this.pawn) == null)
						{
							this.pawnsWithDirectRelationsWithMe.Remove(otherPawn);
						}
					}
					this.directRelations.RemoveAt(i);
					if (this.directRelations.Find((DirectPawnRelation x) => x.otherPawn == otherPawn) == null)
					{
						otherPawn.relations.pawnsWithDirectRelationsWithMe.Remove(this.pawn);
					}
					this.GainedOrLostDirectRelation();
					otherPawn.relations.GainedOrLostDirectRelation();
					return true;
				}
			}
			return false;
		}

		public int OpinionOf(Pawn other)
		{
			if (!other.RaceProps.Humanlike || this.pawn == other)
			{
				return 0;
			}
			if (this.pawn.Dead)
			{
				return 0;
			}
			ProfilerThreadCheck.BeginSample("OpinionOf()");
			int num = 0;
			foreach (PawnRelationDef current in this.pawn.GetRelations(other))
			{
				num += current.opinionOffset;
			}
			if (this.pawn.RaceProps.Humanlike)
			{
				ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
				foreach (ISocialThought current2 in this.pawn.needs.mood.thoughts.DistinctSocialThoughtGroups(other))
				{
					num += thoughts.OpinionOffsetOfThoughtGroup(current2, other);
				}
			}
			if (num != 0)
			{
				float num2 = 1f;
				List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					if (hediffs[i].CurStage != null)
					{
						num2 *= hediffs[i].CurStage.opinionOfOthersFactor;
					}
				}
				num = Mathf.RoundToInt((float)num * num2);
			}
			if (num > 0 && this.pawn.HostileTo(other))
			{
				num = 0;
			}
			ProfilerThreadCheck.EndSample();
			return Mathf.Clamp(num, -100, 100);
		}

		public string OpinionExplanation(Pawn other)
		{
			if (!other.RaceProps.Humanlike || this.pawn == other)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("OpinionOf".Translate(new object[]
			{
				other.LabelShort
			}) + ": " + this.OpinionOf(other).ToStringWithSign());
			string pawnSituationLabel = SocialCardUtility.GetPawnSituationLabel(other, this.pawn);
			if (!pawnSituationLabel.NullOrEmpty())
			{
				stringBuilder.AppendLine(pawnSituationLabel);
			}
			stringBuilder.AppendLine("--------------");
			bool flag = false;
			if (this.pawn.Dead)
			{
				stringBuilder.AppendLine("IAmDead".Translate());
				flag = true;
			}
			else
			{
				IEnumerable<PawnRelationDef> relations = this.pawn.GetRelations(other);
				foreach (PawnRelationDef current in relations)
				{
					stringBuilder.AppendLine(current.GetGenderSpecificLabelCap(other) + ": " + current.opinionOffset.ToStringWithSign());
					flag = true;
				}
				if (this.pawn.RaceProps.Humanlike)
				{
					ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
					foreach (ISocialThought current2 in thoughts.DistinctSocialThoughtGroups(other))
					{
						int num = 1;
						Thought thought = (Thought)current2;
						if (thought.def.IsMemory)
						{
							num = thoughts.memories.NumSocialMemoryThoughtsInGroup((Thought_MemorySocial)current2, other.thingIDNumber);
						}
						stringBuilder.Append(thought.LabelCapSocial);
						if (num != 1)
						{
							stringBuilder.Append(" x" + num);
						}
						stringBuilder.AppendLine(": " + thoughts.OpinionOffsetOfThoughtGroup(current2, other).ToStringWithSign());
						flag = true;
					}
				}
				List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					HediffStage curStage = hediffs[i].CurStage;
					if (curStage != null && curStage.opinionOfOthersFactor != 1f)
					{
						stringBuilder.Append(hediffs[i].LabelBase.CapitalizeFirst());
						if (curStage.opinionOfOthersFactor != 0f)
						{
							stringBuilder.AppendLine(": x" + curStage.opinionOfOthersFactor.ToStringPercent());
						}
						else
						{
							stringBuilder.AppendLine();
						}
						flag = true;
					}
				}
				if (this.pawn.HostileTo(other))
				{
					stringBuilder.AppendLine("Hostile".Translate());
					flag = true;
				}
			}
			if (!flag)
			{
				stringBuilder.AppendLine("NoneBrackets".Translate());
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public float AttractionTo(Pawn otherPawn)
		{
			if (this.pawn.def != otherPawn.def || this.pawn == otherPawn)
			{
				return 0f;
			}
			float num = 1f;
			float num2 = 1f;
			float ageBiologicalYearsFloat = this.pawn.ageTracker.AgeBiologicalYearsFloat;
			float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
			if (this.pawn.gender == Gender.Male)
			{
				if (this.pawn.RaceProps.Humanlike && this.pawn.story.traits.HasTrait(TraitDefOf.Gay))
				{
					if (otherPawn.gender == Gender.Female)
					{
						return 0f;
					}
				}
				else if (otherPawn.gender == Gender.Male)
				{
					return 0f;
				}
				num2 = GenMath.FlatHill(16f, 20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 15f, ageBiologicalYearsFloat2);
			}
			else if (this.pawn.gender == Gender.Female)
			{
				if (this.pawn.RaceProps.Humanlike && this.pawn.story.traits.HasTrait(TraitDefOf.Gay))
				{
					if (otherPawn.gender == Gender.Male)
					{
						return 0f;
					}
				}
				else if (otherPawn.gender == Gender.Female)
				{
					num = 0.15f;
				}
				if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f)
				{
					return 0f;
				}
				if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
				{
					num2 = Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.2f;
				}
				else
				{
					num2 = GenMath.FlatHill(0.2f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 40f, 0.1f, ageBiologicalYearsFloat2);
				}
			}
			float num3 = 1f;
			num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
			num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation));
			num3 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving));
			float num4 = 1f;
			foreach (PawnRelationDef current in this.pawn.GetRelations(otherPawn))
			{
				num4 *= current.attractionFactor;
			}
			int num5 = 0;
			if (otherPawn.RaceProps.Humanlike)
			{
				num5 = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
			}
			float num6 = 1f;
			if (num5 < 0)
			{
				num6 = 0.3f;
			}
			else if (num5 > 0)
			{
				num6 = 2.3f;
			}
			float num7 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
			float num8 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
			return num * num2 * num3 * num4 * num7 * num8 * num6;
		}

		public float CompatibilityWith(Pawn otherPawn)
		{
			if (this.pawn.def != otherPawn.def || this.pawn == otherPawn)
			{
				return 0f;
			}
			float x = Mathf.Abs(this.pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
			float num = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, x);
			num = Mathf.Clamp(num, -0.45f, 0.45f);
			float num2 = this.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
			return num + num2;
		}

		public float ConstantPerPawnsPairCompatibilityOffset(int otherPawnID)
		{
			Rand.PushSeed();
			Rand.Seed = (this.pawn.thingIDNumber ^ otherPawnID) * 37;
			float result = Rand.GaussianAsymmetric(0.3f, 1f, 1.4f);
			Rand.PopSeed();
			return result;
		}

		public void ClearAllRelations()
		{
			List<DirectPawnRelation> list = this.directRelations.ToList<DirectPawnRelation>();
			for (int i = 0; i < list.Count; i++)
			{
				this.RemoveDirectRelation(list[i]);
			}
			List<Pawn> list2 = this.pawnsWithDirectRelationsWithMe.ToList<Pawn>();
			for (int j = 0; j < list2.Count; j++)
			{
				List<DirectPawnRelation> list3 = list2[j].relations.directRelations.ToList<DirectPawnRelation>();
				for (int k = 0; k < list3.Count; k++)
				{
					if (list3[k].otherPawn == this.pawn)
					{
						list2[j].relations.RemoveDirectRelation(list3[k]);
					}
				}
			}
		}

		internal void Notify_PawnDied(DamageInfo? dinfo)
		{
			foreach (Pawn current in this.PotentiallyRelatedPawns)
			{
				if (!current.Dead && current.needs.mood != null)
				{
					current.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
			}
			this.RemoveMySpouseMarriageRelatedThoughts();
			if (this.everSeenByPlayer && !PawnGenerator.IsBeingGenerated(this.pawn))
			{
				foreach (Pawn current2 in this.PotentiallyRelatedPawns)
				{
					if (!current2.Dead && current2.needs.mood != null)
					{
						PawnRelationDef mostImportantRelation = current2.GetMostImportantRelation(this.pawn);
						if (mostImportantRelation != null)
						{
							ThoughtDef genderSpecificDiedThought = mostImportantRelation.GetGenderSpecificDiedThought(this.pawn);
							if (genderSpecificDiedThought != null)
							{
								Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(genderSpecificDiedThought);
								thought_Memory.subject = this.pawn.LabelShort;
								current2.needs.mood.thoughts.memories.TryGainMemoryThought(thought_Memory, null);
							}
						}
					}
				}
				if (dinfo.HasValue)
				{
					Pawn pawn = dinfo.Value.Instigator as Pawn;
					if (pawn != null && pawn != this.pawn)
					{
						foreach (Pawn current3 in this.PotentiallyRelatedPawns)
						{
							if (pawn != current3)
							{
								if (!current3.Dead)
								{
									if (current3.needs.mood != null)
									{
										PawnRelationDef mostImportantRelation2 = current3.GetMostImportantRelation(this.pawn);
										if (mostImportantRelation2 != null)
										{
											ThoughtDef genderSpecificKilledThought = mostImportantRelation2.GetGenderSpecificKilledThought(this.pawn);
											if (genderSpecificKilledThought != null)
											{
												current3.needs.mood.thoughts.memories.TryGainMemoryThought(genderSpecificKilledThought, pawn);
											}
										}
										if (current3.RaceProps.IsFlesh)
										{
											int num = current3.relations.OpinionOf(this.pawn);
											if (num >= 20)
											{
												Thought_MemorySocial thought_MemorySocial = (Thought_MemorySocial)ThoughtMaker.MakeThought(ThoughtDefOf.KilledMyFriend);
												thought_MemorySocial.opinionOffset *= this.GetFriendDiedThoughtPowerFactor(num);
												current3.needs.mood.thoughts.memories.TryGainMemoryThought(thought_MemorySocial, pawn);
											}
											else if (num <= -20)
											{
												Thought_MemorySocial thought_MemorySocial2 = (Thought_MemorySocial)ThoughtMaker.MakeThought(ThoughtDefOf.KilledMyRival);
												thought_MemorySocial2.opinionOffset *= this.GetRivalDiedThoughtPowerFactor(num);
												current3.needs.mood.thoughts.memories.TryGainMemoryThought(thought_MemorySocial2, pawn);
											}
										}
									}
								}
							}
						}
					}
				}
				if (this.pawn.RaceProps.Humanlike)
				{
					foreach (Pawn current4 in Find.MapPawns.AllPawns)
					{
						if (!current4.Dead && current4.RaceProps.IsFlesh && current4.needs.mood != null)
						{
							int num2 = current4.relations.OpinionOf(this.pawn);
							if (num2 >= 20)
							{
								Thought_Memory thought_Memory2 = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.PawnWithGoodOpinionDied);
								thought_Memory2.moodPowerFactor = this.GetFriendDiedThoughtPowerFactor(num2);
								thought_Memory2.subject = this.pawn.LabelShort;
								current4.needs.mood.thoughts.memories.TryGainMemoryThought(thought_Memory2, null);
							}
							else if (num2 <= -20)
							{
								Thought_Memory thought_Memory3 = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.PawnWithBadOpinionDied);
								thought_Memory3.moodPowerFactor = this.GetRivalDiedThoughtPowerFactor(num2);
								thought_Memory3.subject = this.pawn.LabelShort;
								current4.needs.mood.thoughts.memories.TryGainMemoryThought(thought_Memory3, null);
							}
						}
					}
				}
				if (this.pawn.RaceProps.Animal)
				{
					this.SendBondedAnimalDiedLetter();
				}
				else
				{
					this.AffectBondedAnimalsOnMyDeath();
				}
			}
		}

		public void Notify_PawnSold(Pawn playerNegotiator)
		{
			foreach (Pawn current in this.PotentiallyRelatedPawns)
			{
				if (!current.Dead && current.needs.mood != null)
				{
					PawnRelationDef mostImportantRelation = current.GetMostImportantRelation(this.pawn);
					if (mostImportantRelation != null && mostImportantRelation.soldThought != null)
					{
						Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(mostImportantRelation.soldThought);
						Thought_MemorySocial thought_MemorySocial = thought_Memory as Thought_MemorySocial;
						if (thought_MemorySocial != null)
						{
							thought_MemorySocial.SetOtherPawn(playerNegotiator);
						}
						thought_Memory.subject = this.pawn.LabelShort;
						current.needs.mood.thoughts.memories.TryGainMemoryThought(thought_Memory, null);
					}
				}
			}
			this.RemoveMySpouseMarriageRelatedThoughts();
		}

		public void Notify_PawnKidnapped()
		{
			this.RemoveMySpouseMarriageRelatedThoughts();
		}

		public void Notify_RescuedBy(Pawn rescuer)
		{
			if (rescuer.RaceProps.Humanlike && this.canGetRescuedThought)
			{
				this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RescuedMe, rescuer);
				this.canGetRescuedThought = false;
			}
		}

		private float GetFriendDiedThoughtPowerFactor(int opinion)
		{
			return Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(20f, 100f, (float)opinion));
		}

		private float GetRivalDiedThoughtPowerFactor(int opinion)
		{
			return Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(-20f, -100f, (float)opinion));
		}

		private void RemoveMySpouseMarriageRelatedThoughts()
		{
			Pawn spouse = this.pawn.GetSpouse();
			if (spouse != null && !spouse.Dead && spouse.needs.mood != null)
			{
				MemoryThoughtHandler memories = spouse.needs.mood.thoughts.memories;
				memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.GotMarried);
				memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.HoneymoonPhase);
			}
		}

		private void SendBondedAnimalDiedLetter()
		{
			Predicate<Pawn> isAffected = (Pawn x) => !x.Dead && (!x.RaceProps.Humanlike || !x.story.traits.HasTrait(TraitDefOf.Psychopath));
			int num = 0;
			for (int i = 0; i < this.directRelations.Count; i++)
			{
				if (this.directRelations[i].def == PawnRelationDefOf.Bond && isAffected(this.directRelations[i].otherPawn))
				{
					num++;
				}
			}
			if (num == 0)
			{
				return;
			}
			string str;
			if (num == 1)
			{
				Pawn firstDirectRelationPawn = this.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, (Pawn x) => isAffected(x));
				if (this.pawn.Name != null)
				{
					str = "LetterNamedBondedAnimalDied".Translate(new object[]
					{
						this.pawn.KindLabel,
						this.pawn.Name.ToStringShort,
						firstDirectRelationPawn.LabelShort
					});
				}
				else
				{
					str = "LetterBondedAnimalDied".Translate(new object[]
					{
						this.pawn.KindLabel,
						firstDirectRelationPawn.LabelShort
					});
				}
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int j = 0; j < this.directRelations.Count; j++)
				{
					if (this.directRelations[j].def == PawnRelationDefOf.Bond && isAffected(this.directRelations[j].otherPawn))
					{
						stringBuilder.AppendLine("  - " + this.directRelations[j].otherPawn.LabelShort);
					}
				}
				if (this.pawn.Name != null)
				{
					str = "LetterNamedBondedAnimalDiedMulti".Translate(new object[]
					{
						this.pawn.KindLabel,
						this.pawn.Name.ToStringShort,
						stringBuilder.ToString().TrimEndNewlines()
					});
				}
				else
				{
					str = "LetterBondedAnimalDiedMulti".Translate(new object[]
					{
						this.pawn.KindLabel,
						stringBuilder.ToString().TrimEndNewlines()
					});
				}
			}
			Find.LetterStack.ReceiveLetter("LetterLabelBondedAnimalDied".Translate(), str.CapitalizeFirst(), LetterType.BadNonUrgent, this.pawn.Position, null);
		}

		private void AffectBondedAnimalsOnMyDeath()
		{
			int num = 0;
			Pawn pawn = null;
			for (int i = 0; i < this.directRelations.Count; i++)
			{
				if (this.directRelations[i].def == PawnRelationDefOf.Bond && this.directRelations[i].otherPawn.Spawned)
				{
					pawn = this.directRelations[i].otherPawn;
					num++;
					float value = Rand.Value;
					MentalStateDef stateDef;
					if (value < 0.25f)
					{
						stateDef = MentalStateDefOf.WanderSad;
					}
					if (value < 0.5f)
					{
						stateDef = MentalStateDefOf.WanderPsychotic;
					}
					else if (value < 0.75f)
					{
						stateDef = MentalStateDefOf.Berserk;
					}
					else
					{
						stateDef = MentalStateDefOf.Manhunter;
					}
					this.directRelations[i].otherPawn.mindState.mentalStateHandler.TryStartMentalState(stateDef, null, true);
				}
			}
			if (num == 1)
			{
				string str;
				if (pawn.Name != null && !pawn.Name.Numerical)
				{
					str = "MessageNamedBondedAnimalMentalBreak".Translate(new object[]
					{
						pawn.KindLabel,
						pawn.Name.ToStringShort,
						this.pawn.LabelShort
					});
				}
				else
				{
					str = "MessageBondedAnimalMentalBreak".Translate(new object[]
					{
						pawn.KindLabel,
						this.pawn.LabelShort
					});
				}
				Messages.Message(str.CapitalizeFirst(), pawn, MessageSound.SeriousAlert);
			}
			else if (num > 1)
			{
				Messages.Message("MessageBondedAnimalsMentalBreak".Translate(new object[]
				{
					num,
					this.pawn.LabelShort
				}).CapitalizeFirst(), pawn, MessageSound.SeriousAlert);
			}
		}

		private void Tick_CheckStartMarriageCeremony()
		{
			if (!this.pawn.Spawned || this.pawn.RaceProps.Animal)
			{
				return;
			}
			if (this.pawn.IsHashIntervalTick(1017))
			{
				int ticksGame = Find.TickManager.TicksGame;
				for (int i = 0; i < this.directRelations.Count; i++)
				{
					float num = (float)(ticksGame - this.directRelations[i].startTicks) / 60000f;
					if (this.directRelations[i].def == PawnRelationDefOf.Fiance && this.pawn.thingIDNumber < this.directRelations[i].otherPawn.thingIDNumber && num > 10f && Rand.MTBEventOccurs(2f, 60000f, 1017f) && MarriageCeremonyUtility.AcceptableMapConditionsToStartCeremony() && MarriageCeremonyUtility.FianceReadyToStartCeremony(this.pawn) && MarriageCeremonyUtility.FianceReadyToStartCeremony(this.directRelations[i].otherPawn))
					{
						Find.VoluntarilyJoinableLordsStarter.TryStartMarriageCeremony(this.pawn, this.directRelations[i].otherPawn);
					}
				}
			}
		}

		private void Tick_CheckDevelopBondRelation()
		{
			if (!this.pawn.Spawned || !this.pawn.RaceProps.Animal || this.pawn.Faction != Faction.OfPlayer || this.pawn.playerSettings.master == null)
			{
				return;
			}
			Pawn master = this.pawn.playerSettings.master;
			if (this.pawn.IsHashIntervalTick(2500) && this.pawn.Position.InHorDistOf(master.Position, 12f) && GenSight.LineOfSight(this.pawn.Position, master.Position, false))
			{
				RelationsUtility.TryDevelopBondRelation(master, this.pawn, 0.01f);
			}
		}

		private void GainedOrLostDirectRelation()
		{
			if (Current.ProgramState == ProgramState.MapPlaying && !this.pawn.Dead && this.pawn.needs.mood != null)
			{
				this.pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
		}
	}
}
