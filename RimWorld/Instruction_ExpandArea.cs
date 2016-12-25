using System;
using Verse;

namespace RimWorld
{
	public abstract class Instruction_ExpandArea : Lesson_Instruction
	{
		private int startingAreaCount = -1;

		protected abstract Area MyArea
		{
			get;
		}

		protected override float ProgressPercent
		{
			get
			{
				return (float)(this.MyArea.TrueCount - this.startingAreaCount) / (float)this.def.targetCount;
			}
		}

		public override void OnActivated()
		{
			base.OnActivated();
			this.startingAreaCount = this.MyArea.TrueCount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.startingAreaCount, "startingAreaCount", 0, false);
		}

		public override void LessonUpdate()
		{
			if (this.ProgressPercent > 0.999f)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
