using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RecruitAttempt : InteractionWorker
	{
		private const float BaseResistanceReductionPerInteraction = 1f;

		private static readonly SimpleCurve ResistanceImpactFactorCurve_Mood = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0.2f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(1f, 1.5f),
				true
			}
		};

		private static readonly SimpleCurve ResistanceImpactFactorCurve_Opinion = new SimpleCurve
		{
			{
				new CurvePoint(-100f, 0.5f),
				true
			},
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(100f, 1.5f),
				true
			}
		};

		private const float RecruitChancePerNegotiatingAbility = 0.5f;

		private const float MaxMoodForWarning = 0.4f;

		private static readonly SimpleCurve RecruitChanceFactorCurve_Mood = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0.2f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(1f, 2f),
				true
			}
		};

		private const float MaxOpinionForWarning = -0.01f;

		private static readonly SimpleCurve RecruitChanceFactorCurve_Opinion = new SimpleCurve
		{
			{
				new CurvePoint(-100f, 0.5f),
				true
			},
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(100f, 2f),
				true
			}
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_RecruitDifficulty = new SimpleCurve
		{
			{
				new CurvePoint(0f, 2f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(1f, 0.02f),
				true
			}
		};

		private const float WildmanWildness = 0.2f;

		private static readonly SimpleCurve TameChanceFactorCurve_Wildness = new SimpleCurve
		{
			{
				new CurvePoint(1f, 0f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(0f, 2f),
				true
			}
		};

		private const float TameChanceFactor_Bonded = 4f;

		private const float ChanceToDevelopBondRelationOnTamed = 0.01f;

		private const int MenagerieTaleThreshold = 5;

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef)
		{
			letterText = null;
			letterLabel = null;
			letterDef = null;
			if (recipient.mindState.CheckStartMentalStateBecauseRecruitAttempted(initiator))
			{
				return;
			}
			bool flag = recipient.AnimalOrWildMan() && !recipient.IsPrisoner;
			float x = (float)((recipient.relations == null) ? 0 : recipient.relations.OpinionOf(initiator));
			bool flag2 = initiator.InspirationDef == InspirationDefOf.Inspired_Recruitment && !flag && recipient.guest.interactionMode != PrisonerInteractionModeDefOf.ReduceResistance;
			if (!flag && recipient.guest.resistance > 0f && !flag2)
			{
				float num = 1f;
				num *= initiator.GetStatValue(StatDefOf.NegotiationAbility, true);
				num *= InteractionWorker_RecruitAttempt.ResistanceImpactFactorCurve_Mood.Evaluate(recipient.needs.mood.CurLevelPercentage);
				num *= InteractionWorker_RecruitAttempt.ResistanceImpactFactorCurve_Opinion.Evaluate(x);
				num = Mathf.Min(num, recipient.guest.resistance);
				float resistance = recipient.guest.resistance;
				recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - num);
				string text = "TextMote_ResistanceReduced".Translate(new object[]
				{
					resistance.ToString("F1"),
					recipient.guest.resistance.ToString("F1")
				});
				if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
				{
					text = text + "\n(" + "lowMood".Translate() + ")";
				}
				if (recipient.relations != null && (float)recipient.relations.OpinionOf(initiator) < -0.01f)
				{
					text = text + "\n(" + "lowOpinion".Translate() + ")";
				}
				MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
				if (recipient.guest.resistance == 0f)
				{
					string text2 = "MessagePrisonerResistanceBroken".Translate(new object[]
					{
						recipient.LabelShort,
						initiator.LabelShort
					});
					if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
					{
						text2 = text2 + " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
					}
					Messages.Message(text2, recipient, MessageTypeDefOf.PositiveEvent, true);
				}
			}
			else
			{
				float num2;
				if (flag)
				{
					num2 = initiator.GetStatValue(StatDefOf.TameAnimalChance, true);
					float x2 = (!recipient.IsWildMan()) ? recipient.RaceProps.wildness : 0.2f;
					num2 *= InteractionWorker_RecruitAttempt.TameChanceFactorCurve_Wildness.Evaluate(x2);
					if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
					{
						num2 *= 4f;
					}
				}
				else if (flag2 || DebugSettings.instantRecruit)
				{
					num2 = 1f;
				}
				else
				{
					num2 = initiator.GetStatValue(StatDefOf.NegotiationAbility, true) * 0.5f;
					float x3 = recipient.RecruitDifficulty(initiator.Faction);
					num2 *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_RecruitDifficulty.Evaluate(x3);
					num2 *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Opinion.Evaluate(x);
					if (recipient.needs.mood != null)
					{
						float curLevel = recipient.needs.mood.CurLevel;
						num2 *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Mood.Evaluate(curLevel);
					}
				}
				if (Rand.Chance(num2))
				{
					InteractionWorker_RecruitAttempt.DoRecruit(initiator, recipient, num2, true);
					if (flag2)
					{
						initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Recruitment);
					}
					extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
				}
				else
				{
					string text3 = (!flag) ? "TextMote_RecruitFail".Translate(new object[]
					{
						num2.ToStringPercent()
					}) : "TextMote_TameFail".Translate(new object[]
					{
						num2.ToStringPercent()
					});
					if (!flag)
					{
						if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
						{
							text3 = text3 + "\n(" + "lowMood".Translate() + ")";
						}
						if (recipient.relations != null && (float)recipient.relations.OpinionOf(initiator) < -0.01f)
						{
							text3 = text3 + "\n(" + "lowOpinion".Translate() + ")";
						}
					}
					MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text3, 8f);
					extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
				}
			}
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
		{
			recruitChance = Mathf.Clamp01(recruitChance);
			string text = recruitee.LabelIndefinite();
			if (recruitee.guest != null)
			{
				recruitee.guest.SetGuestStatus(null, false);
			}
			bool flag = recruitee.Name != null;
			if (recruitee.Faction != recruiter.Faction)
			{
				recruitee.SetFaction(recruiter.Faction, recruiter);
			}
			if (recruitee.RaceProps.Humanlike)
			{
				if (useAudiovisualEffects)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelMessageRecruitSuccess".Translate(), "MessageRecruitSuccess".Translate(new object[]
					{
						recruiter,
						recruitee,
						recruitChance.ToStringPercent()
					}), LetterDefOf.PositiveEvent, recruitee, null, null);
				}
				TaleRecorder.RecordTale(TaleDefOf.Recruited, new object[]
				{
					recruiter,
					recruitee
				});
				recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
				recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
			}
			else
			{
				if (useAudiovisualEffects)
				{
					if (!flag)
					{
						Messages.Message("MessageTameAndNameSuccess".Translate(new object[]
						{
							recruiter.LabelShort,
							text,
							recruitChance.ToStringPercent(),
							recruitee.Name.ToStringFull
						}).AdjustedFor(recruitee, "PAWN"), recruitee, MessageTypeDefOf.PositiveEvent, true);
					}
					else
					{
						Messages.Message("MessageTameSuccess".Translate(new object[]
						{
							recruiter.LabelShort,
							text,
							recruitChance.ToStringPercent()
						}), recruitee, MessageTypeDefOf.PositiveEvent, true);
					}
					if (recruiter.Spawned && recruitee.Spawned)
					{
						MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(new object[]
						{
							recruitChance.ToStringPercent()
						}), 8f);
					}
				}
				recruiter.records.Increment(RecordDefOf.AnimalsTamed);
				RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
				float chance = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
				if (Rand.Chance(chance) || recruitee.IsWildMan())
				{
					TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, new object[]
					{
						recruiter,
						recruitee
					});
				}
				if (PawnsFinder.AllMapsWorldAndTemporary_Alive.Count((Pawn p) => p.playerSettings != null && p.playerSettings.Master == recruiter) >= 5)
				{
					TaleRecorder.RecordTale(TaleDefOf.IncreasedMenagerie, new object[]
					{
						recruiter,
						recruitee
					});
				}
			}
			if (recruitee.caller != null)
			{
				recruitee.caller.DoCall();
			}
		}
	}
}
