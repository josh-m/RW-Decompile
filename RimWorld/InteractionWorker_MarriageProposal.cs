using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_MarriageProposal : InteractionWorker
	{
		private const float BaseSelectionWeight = 0.4f;

		private const float BaseAcceptanceChance = 0.9f;

		private const float BreakupChanceOnRejection = 0.4f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			DirectPawnRelation directRelation = initiator.relations.GetDirectRelation(PawnRelationDefOf.Lover, recipient);
			if (directRelation == null)
			{
				return 0f;
			}
			Pawn spouse = recipient.GetSpouse();
			Pawn spouse2 = initiator.GetSpouse();
			if ((spouse != null && !spouse.Dead) || (spouse2 != null && !spouse2.Dead))
			{
				return 0f;
			}
			float num = 0.4f;
			int ticksGame = Find.TickManager.TicksGame;
			float value = (float)(ticksGame - directRelation.startTicks) / 60000f;
			num *= Mathf.InverseLerp(0f, 60f, value);
			num *= Mathf.InverseLerp(0f, 60f, (float)initiator.relations.OpinionOf(recipient));
			if (recipient.relations.OpinionOf(initiator) < 0)
			{
				num *= 0.3f;
			}
			if (initiator.gender == Gender.Female)
			{
				num *= 0.2f;
			}
			return num;
		}

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			float num = this.AcceptanceChance(initiator, recipient);
			bool flag = Rand.Value < num;
			bool brokeUp = false;
			if (flag)
			{
				initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.Fiance, recipient);
				initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.RejectedMyProposal, initiator);
				initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDefOf.IRejectedTheirProposal, initiator);
				extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalAccepted);
			}
			else
			{
				initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RejectedMyProposal, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.IRejectedTheirProposal, initiator);
				extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejected);
				if (Rand.Value < 0.4f)
				{
					initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
					initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
					brokeUp = true;
					extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejectedBrokeUp);
				}
			}
			if (initiator.IsColonist || recipient.IsColonist)
			{
				this.SendLetter(initiator, recipient, flag, brokeUp);
			}
		}

		public float AcceptanceChance(Pawn initiator, Pawn recipient)
		{
			float num = 0.9f;
			num *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, (float)recipient.relations.OpinionOf(initiator)));
			return Mathf.Clamp01(num);
		}

		private void SendLetter(Pawn initiator, Pawn recipient, bool accepted, bool brokeUp)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string label;
			LetterType type;
			if (accepted)
			{
				label = "LetterLabelAcceptedProposal".Translate();
				type = LetterType.Good;
				stringBuilder.AppendLine("LetterAcceptedProposal".Translate(new object[]
				{
					initiator,
					recipient
				}));
			}
			else
			{
				label = "LetterLabelRejectedProposal".Translate();
				type = LetterType.BadNonUrgent;
				stringBuilder.AppendLine("LetterRejectedProposal".Translate(new object[]
				{
					initiator,
					recipient
				}));
				if (brokeUp)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(new object[]
					{
						initiator,
						recipient
					}));
				}
			}
			Find.LetterStack.ReceiveLetter(label, stringBuilder.ToString().TrimEndNewlines(), type, initiator, null);
		}
	}
}
