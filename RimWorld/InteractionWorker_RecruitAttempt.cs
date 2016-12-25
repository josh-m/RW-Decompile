using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RecruitAttempt : InteractionWorker
	{
		private const float MinRecruitChance = 0.005f;

		private const float BondRelationChanceFactor = 4f;

		private static readonly SimpleCurve RecruitChanceFactorCurve_Wildness = new SimpleCurve
		{
			new CurvePoint(1f, 0f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(0f, 2f)
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_Opinion = new SimpleCurve
		{
			new CurvePoint(-50f, 0f),
			new CurvePoint(50f, 1f),
			new CurvePoint(100f, 2f)
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_Mood = new SimpleCurve
		{
			new CurvePoint(0f, 0.25f),
			new CurvePoint(0.1f, 0.25f),
			new CurvePoint(0.25f, 1f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 1.5f)
		};

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			if (recipient.mindState.CheckStartMentalStateBecauseRecruitAttempted(initiator))
			{
				return;
			}
			float num = 1f;
			if (DebugSettings.instantRecruit)
			{
				num = 1f;
			}
			else
			{
				num *= ((!recipient.RaceProps.Humanlike) ? initiator.GetStatValue(StatDefOf.TameAnimalChance, true) : initiator.GetStatValue(StatDefOf.RecruitPrisonerChance, true));
				float num2 = (!recipient.RaceProps.Humanlike) ? InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Wildness.Evaluate(recipient.RaceProps.wildness) : (1f - recipient.RecruitDifficulty(initiator.Faction, true));
				num *= num2;
				if (recipient.RaceProps.Humanlike)
				{
					float x = (float)recipient.relations.OpinionOf(initiator);
					num *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Opinion.Evaluate(x);
					if (recipient.needs.mood != null)
					{
						float curLevel = recipient.needs.mood.CurLevel;
						num *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Mood.Evaluate(curLevel);
					}
				}
				if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
				{
					num *= 4f;
				}
				num = Mathf.Clamp(num, 0.005f, 1f);
			}
			if (Rand.Value < num)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(initiator, recipient, num, true);
				extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
			}
			else
			{
				string text;
				if (recipient.RaceProps.Humanlike)
				{
					text = "TextMote_RecruitFail".Translate(new object[]
					{
						num.ToStringPercent()
					});
				}
				else
				{
					text = "TextMote_TameFail".Translate(new object[]
					{
						num.ToStringPercent()
					});
				}
				MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
				extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
			}
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
		{
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
					}), LetterType.Good, recruitee, null);
				}
				TaleRecorder.RecordTale(TaleDefOf.Recruited, new object[]
				{
					recruiter,
					recruitee
				});
				recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
				recruitee.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RecruitedMe, recruiter);
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
						}).AdjustedFor(recruitee), recruitee, MessageSound.Benefit);
					}
					else
					{
						Messages.Message("MessageTameSuccess".Translate(new object[]
						{
							recruiter.LabelShort,
							text,
							recruitChance.ToStringPercent()
						}), recruitee, MessageSound.Benefit);
					}
					MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(new object[]
					{
						recruitChance.ToStringPercent()
					}), 8f);
				}
				recruiter.records.Increment(RecordDefOf.AnimalsTamed);
				RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
				float num = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
				if (Rand.Value < num)
				{
					TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, new object[]
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
