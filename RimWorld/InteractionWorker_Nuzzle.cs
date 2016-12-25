using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_Nuzzle : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			this.AddNuzzledThought(initiator, recipient);
			this.TryGiveName(initiator, recipient);
		}

		private void AddNuzzledThought(Pawn initiator, Pawn recipient)
		{
			Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.Nuzzled);
			recipient.needs.mood.thoughts.memories.TryGainMemoryThought(newThought, null);
		}

		private void TryGiveName(Pawn initiator, Pawn recipient)
		{
			if ((initiator.Name == null || initiator.Name.Numerical) && Rand.Value < initiator.RaceProps.nameOnNuzzleChance)
			{
				PawnUtility.GiveNameBecauseOfNuzzle(recipient, initiator);
			}
		}
	}
}
