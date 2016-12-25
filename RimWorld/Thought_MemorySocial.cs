using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_MemorySocial : Thought_Memory, ISocialThought
	{
		public int otherPawnID = -1;

		public float opinionOffset;

		public override bool ShouldDiscard
		{
			get
			{
				return this.opinionOffset == 0f || base.ShouldDiscard;
			}
		}

		public override bool VisibleInNeedsTab
		{
			get
			{
				return base.VisibleInNeedsTab && this.MoodOffset() != 0f;
			}
		}

		private float AgePct
		{
			get
			{
				return (float)this.age / (float)this.def.DurationTicks;
			}
		}

		private float AgeFactor
		{
			get
			{
				return Mathf.InverseLerp(1f, this.def.lerpOpinionToZeroAfterDurationPct, this.AgePct);
			}
		}

		public void SetOtherPawn(Pawn pawn)
		{
			this.otherPawnID = pawn.thingIDNumber;
		}

		public virtual float OpinionOffset()
		{
			if (this.ShouldDiscard)
			{
				return 0f;
			}
			return this.opinionOffset * this.AgeFactor;
		}

		public int OtherPawnID()
		{
			return this.otherPawnID;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.otherPawnID, "otherPawnID", 0, false);
			Scribe_Values.LookValue<float>(ref this.opinionOffset, "opinionOffset", 0f, false);
		}

		public override void Init()
		{
			base.Init();
			this.opinionOffset = base.CurStage.baseOpinionOffset;
		}

		public override bool TryMergeWithExistingThought()
		{
			return false;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_MemorySocial thought_MemorySocial = other as Thought_MemorySocial;
			return thought_MemorySocial != null && base.GroupsWith(other) && this.otherPawnID == thought_MemorySocial.otherPawnID;
		}
	}
}
