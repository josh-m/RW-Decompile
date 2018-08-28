using System;
using Verse;

namespace RimWorld
{
	public class Thought_Situational : Thought
	{
		private int curStageIndex = -1;

		protected string reason;

		public bool Active
		{
			get
			{
				return this.curStageIndex >= 0;
			}
		}

		public override int CurStageIndex
		{
			get
			{
				return this.curStageIndex;
			}
		}

		public override string LabelCap
		{
			get
			{
				if (!this.reason.NullOrEmpty())
				{
					return string.Format(base.CurStage.label, this.reason).CapitalizeFirst();
				}
				return base.LabelCap;
			}
		}

		public void RecalculateState()
		{
			ThoughtState thoughtState = this.CurrentStateInternal();
			if (thoughtState.ActiveFor(this.def))
			{
				this.curStageIndex = thoughtState.StageIndexFor(this.def);
				this.reason = thoughtState.Reason;
			}
			else
			{
				this.curStageIndex = -1;
			}
		}

		protected virtual ThoughtState CurrentStateInternal()
		{
			return this.def.Worker.CurrentState(this.pawn);
		}
	}
}
