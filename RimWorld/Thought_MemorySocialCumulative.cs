using System;
using System.Collections.Generic;
using UnityEngine;

namespace RimWorld
{
	public class Thought_MemorySocialCumulative : Thought_MemorySocial
	{
		private const float OpinionOffsetChangePerDay = 1f;

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

		public override bool TryMergeWithExistingThought()
		{
			ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
			List<Thought> thoughts2 = thoughts.Thoughts;
			for (int i = 0; i < thoughts2.Count; i++)
			{
				if (thoughts2[i].def == this.def)
				{
					Thought_MemorySocialCumulative thought_MemorySocialCumulative = (Thought_MemorySocialCumulative)thoughts2[i];
					if (this.otherPawnID == thought_MemorySocialCumulative.otherPawnID)
					{
						thought_MemorySocialCumulative.opinionOffset += this.opinionOffset;
						return true;
					}
				}
			}
			return false;
		}
	}
}
