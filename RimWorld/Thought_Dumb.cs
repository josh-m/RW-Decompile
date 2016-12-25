using System;
using Verse;

namespace RimWorld
{
	public class Thought_Dumb : Thought
	{
		private int forcedStage;

		public override int CurStageIndex
		{
			get
			{
				return this.forcedStage;
			}
		}

		public void SetForcedStage(int stageIndex)
		{
			this.forcedStage = stageIndex;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.forcedStage, "stageIndex", 0, false);
		}
	}
}
