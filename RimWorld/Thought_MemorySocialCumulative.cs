using System;
using System.Collections.Generic;
using UnityEngine;

namespace RimWorld
{
	public class Thought_MemorySocialCumulative : Thought_MemorySocial
	{
		private const float OpinionOffsetChangePerDay = 1f;

		private static List<Thought> tmpThoughts = new List<Thought>();

		public override bool ShouldDiscard
		{
			get
			{
				return this.opinionOffset == 0f;
			}
		}

		public override float OpinionOffset()
		{
			if (this.ShouldDiscard)
			{
				return 0f;
			}
			return Mathf.Min(this.opinionOffset, this.def.maxCumulatedOpinionOffset);
		}

		public override void ThoughtInterval()
		{
			base.ThoughtInterval();
			if (this.age >= 60000)
			{
				if (this.opinionOffset < 0f)
				{
					this.opinionOffset += 1f;
					if (this.opinionOffset > 0f)
					{
						this.opinionOffset = 0f;
					}
				}
				else if (this.opinionOffset > 0f)
				{
					this.opinionOffset -= 1f;
					if (this.opinionOffset < 0f)
					{
						this.opinionOffset = 0f;
					}
				}
				this.age = 0;
			}
		}

		public override bool TryMergeWithExistingThought(out bool showBubble)
		{
			showBubble = false;
			ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
			thoughts.GetMainThoughts(Thought_MemorySocialCumulative.tmpThoughts);
			for (int i = 0; i < Thought_MemorySocialCumulative.tmpThoughts.Count; i++)
			{
				if (Thought_MemorySocialCumulative.tmpThoughts[i].def == this.def)
				{
					Thought_MemorySocialCumulative thought_MemorySocialCumulative = (Thought_MemorySocialCumulative)Thought_MemorySocialCumulative.tmpThoughts[i];
					if (this.otherPawn == thought_MemorySocialCumulative.otherPawn)
					{
						thought_MemorySocialCumulative.opinionOffset += this.opinionOffset;
						Thought_MemorySocialCumulative.tmpThoughts.Clear();
						return true;
					}
				}
			}
			Thought_MemorySocialCumulative.tmpThoughts.Clear();
			return false;
		}
	}
}
