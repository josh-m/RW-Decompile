using System;
using Verse;

namespace RimWorld
{
	public class Thought_Memory : Thought
	{
		public float moodPowerFactor = 1f;

		public Pawn otherPawn;

		public int age;

		private int forcedStage;

		public override bool VisibleInNeedsTab
		{
			get
			{
				return base.VisibleInNeedsTab && !this.ShouldDiscard;
			}
		}

		public override int CurStageIndex
		{
			get
			{
				return this.forcedStage;
			}
		}

		public virtual bool ShouldDiscard
		{
			get
			{
				return this.age > this.def.DurationTicks;
			}
		}

		public override string LabelCap
		{
			get
			{
				if (this.otherPawn != null)
				{
					return string.Format(base.CurStage.label, this.otherPawn.LabelShort).CapitalizeFirst();
				}
				return base.LabelCap;
			}
		}

		public void SetForcedStage(int stageIndex)
		{
			this.forcedStage = stageIndex;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.LookReference<Pawn>(ref this.otherPawn, "otherPawn", true);
			Scribe_Values.LookValue<float>(ref this.moodPowerFactor, "moodPowerFactor", 1f, false);
			Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
			Scribe_Values.LookValue<int>(ref this.forcedStage, "stageIndex", 0, false);
		}

		public virtual void ThoughtInterval()
		{
			this.age += 150;
		}

		public void Renew()
		{
			this.age = 0;
		}

		public override bool TryMergeWithExistingThought(out bool showBubble)
		{
			ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
			if (thoughts.memories.NumMemoryThoughtsInGroup(this) >= this.def.stackLimit)
			{
				Thought_Memory thought_Memory = thoughts.memories.OldestMemoryThoughtInGroup(this);
				if (thought_Memory != null)
				{
					showBubble = (thought_Memory.age > thought_Memory.def.DurationTicks / 2);
					thought_Memory.Renew();
					return true;
				}
			}
			showBubble = true;
			return false;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_Memory thought_Memory = other as Thought_Memory;
			return thought_Memory != null && base.GroupsWith(other) && (this.otherPawn == thought_Memory.otherPawn || this.LabelCap == thought_Memory.LabelCap);
		}

		public override float MoodOffset()
		{
			float num = base.MoodOffset();
			return num * this.moodPowerFactor;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.def.defName,
				", moodPowerFactor=",
				this.moodPowerFactor,
				", age=",
				this.age,
				")"
			});
		}
	}
}
