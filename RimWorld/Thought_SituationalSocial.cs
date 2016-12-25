using System;
using Verse;

namespace RimWorld
{
	public class Thought_SituationalSocial : Thought_Situational, ISocialThought
	{
		public Pawn otherPawn;

		public override bool VisibleInNeedsTab
		{
			get
			{
				return base.VisibleInNeedsTab && this.MoodOffset() != 0f;
			}
		}

		public virtual float OpinionOffset()
		{
			return base.CurStage.baseOpinionOffset;
		}

		public int OtherPawnID()
		{
			return this.otherPawn.thingIDNumber;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_SituationalSocial thought_SituationalSocial = other as Thought_SituationalSocial;
			return thought_SituationalSocial != null && base.GroupsWith(other) && this.otherPawn == thought_SituationalSocial.otherPawn;
		}

		protected override ThoughtState CurrentStateInternal()
		{
			return this.def.Worker.CurrentSocialState(this.pawn, this.otherPawn);
		}
	}
}
