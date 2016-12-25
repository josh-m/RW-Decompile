using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RomanceAttempt : InteractionWorker
	{
		private const float MinAttractionForRomanceAttempt = 0.25f;

		private const int MinOpinionForRomanceAttempt = 5;

		private const float BaseSelectionWeight = 1.15f;

		private const float BaseSuccessChance = 0.6f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
			{
				return 0f;
			}
			float num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
			if (num < 0.25f)
			{
				return 0f;
			}
			int num2 = initiator.relations.OpinionOf(recipient);
			if (num2 < 5)
			{
				return 0f;
			}
			if (recipient.relations.OpinionOf(initiator) < 5)
			{
				return 0f;
			}
			float num3 = 1f;
			Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
			if (pawn != null)
			{
				float value = (float)initiator.relations.OpinionOf(pawn);
				num3 = Mathf.InverseLerp(50f, -50f, value);
			}
			float num4 = (!initiator.story.traits.HasTrait(TraitDefOf.Gay)) ? ((initiator.gender != Gender.Female) ? 1f : 0.15f) : 1f;
			float num5 = Mathf.InverseLerp(0.25f, 1f, num);
			float num6 = Mathf.InverseLerp(5f, 100f, (float)num2);
			float num7 = (initiator.gender != recipient.gender != (initiator.story.traits.HasTrait(TraitDefOf.Gay) != recipient.story.traits.HasTrait(TraitDefOf.Gay))) ? 1f : 0.15f;
			return 1.15f * num4 * num5 * num6 * num3 * num7;
		}

		public float SuccessChance(Pawn initiator, Pawn recipient)
		{
			float num = 0.6f;
			num *= recipient.relations.SecondaryRomanceChanceFactor(initiator);
			num *= Mathf.InverseLerp(5f, 100f, (float)recipient.relations.OpinionOf(initiator));
			float num2 = 1f;
			Pawn pawn = null;
			if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
			{
				pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
				num2 = 0.6f;
			}
			else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
			{
				pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
				num2 = 0.1f;
			}
			else if (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead)
			{
				pawn = recipient.GetSpouse();
				num2 = 0.3f;
			}
			if (pawn != null)
			{
				num2 *= Mathf.InverseLerp(100f, 0f, (float)recipient.relations.OpinionOf(pawn));
				num2 *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
			}
			num *= num2;
			return Mathf.Clamp01(num);
		}

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			if (Rand.Value < this.SuccessChance(initiator, recipient))
			{
				List<Pawn> list;
				this.BreakLoverAndFianceRelations(initiator, out list);
				List<Pawn> list2;
				this.BreakLoverAndFianceRelations(recipient, out list2);
				for (int i = 0; i < list.Count; i++)
				{
					this.TryAddCheaterThought(list[i], initiator);
				}
				for (int j = 0; j < list2.Count; j++)
				{
					this.TryAddCheaterThought(list2[j], recipient);
				}
				initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
				TaleRecorder.RecordTale(TaleDefOf.BecameLover, new object[]
				{
					initiator,
					recipient
				});
				initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, initiator);
				initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, recipient);
				initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
				if (initiator.IsColonist || recipient.IsColonist)
				{
					this.SendNewLoversLetter(initiator, recipient, list, list2);
				}
				extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptAccepted);
				LovePartnerRelationUtility.TryToShareBed(initiator, recipient);
			}
			else
			{
				initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
				if (recipient.relations.OpinionOf(initiator) <= 0)
				{
					recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
				}
				extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptRejected);
			}
		}

		private void BreakLoverAndFianceRelations(Pawn pawn, out List<Pawn> oldLoversAndFiances)
		{
			oldLoversAndFiances = new List<Pawn>();
			while (true)
			{
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
				if (firstDirectRelationPawn != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
					oldLoversAndFiances.Add(firstDirectRelationPawn);
				}
				else
				{
					Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
					if (firstDirectRelationPawn2 == null)
					{
						break;
					}
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
					oldLoversAndFiances.Add(firstDirectRelationPawn2);
				}
			}
		}

		private void TryAddCheaterThought(Pawn pawn, Pawn cheater)
		{
			if (pawn.Dead)
			{
				return;
			}
			pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.CheatedOnMe, cheater);
		}

		private void SendNewLoversLetter(Pawn initiator, Pawn recipient, List<Pawn> initiatorOldLoversAndFiances, List<Pawn> recipientOldLoversAndFiances)
		{
			bool flag = false;
			string label;
			LetterType type;
			Pawn t;
			if ((initiator.GetSpouse() != null && !initiator.GetSpouse().Dead) || (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead))
			{
				label = "LetterLabelAffair".Translate();
				type = LetterType.BadNonUrgent;
				if (initiator.GetSpouse() != null && !initiator.GetSpouse().Dead)
				{
					t = initiator;
				}
				else
				{
					t = recipient;
				}
				flag = true;
			}
			else
			{
				label = "LetterLabelNewLovers".Translate();
				type = LetterType.Good;
				t = initiator;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (flag)
			{
				if (initiator.GetSpouse() != null)
				{
					stringBuilder.AppendLine("LetterAffair".Translate(new object[]
					{
						initiator.LabelShort,
						initiator.GetSpouse().LabelShort,
						recipient.LabelShort
					}));
				}
				if (recipient.GetSpouse() != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("LetterAffair".Translate(new object[]
					{
						recipient.LabelShort,
						recipient.GetSpouse().LabelShort,
						initiator.LabelShort
					}));
				}
			}
			else
			{
				stringBuilder.AppendLine("LetterNewLovers".Translate(new object[]
				{
					initiator.LabelShort,
					recipient.LabelShort
				}));
			}
			for (int i = 0; i < initiatorOldLoversAndFiances.Count; i++)
			{
				if (!initiatorOldLoversAndFiances[i].Dead)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(new object[]
					{
						initiator.LabelShort,
						initiatorOldLoversAndFiances[i].LabelShort
					}));
				}
			}
			for (int j = 0; j < recipientOldLoversAndFiances.Count; j++)
			{
				if (!recipientOldLoversAndFiances[j].Dead)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(new object[]
					{
						recipient.LabelShort,
						recipientOldLoversAndFiances[j].LabelShort
					}));
				}
			}
			Find.LetterStack.ReceiveLetter(label, stringBuilder.ToString().TrimEndNewlines(), type, t, null);
		}
	}
}
