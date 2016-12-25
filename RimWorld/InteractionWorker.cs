using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InteractionWorker
	{
		public virtual float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 0f;
		}

		public virtual void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
		}
	}
}
