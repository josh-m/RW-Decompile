using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_Situational : Thought
	{
		private int curStageIndex = -1;

		protected string reason;

		private static List<Thought> tmpThoughts = new List<Thought>();

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
			if (thoughtState.Active)
			{
				this.curStageIndex = Mathf.Min(thoughtState.StageIndex, this.def.stages.Count - 1);
				this.reason = thoughtState.Reason;
			}
			else
			{
				this.curStageIndex = -1;
			}
		}

		public override bool TryMergeWithExistingThought(out bool showBubble)
		{
			showBubble = false;
			this.pawn.needs.mood.thoughts.GetMainThoughts(Thought_Situational.tmpThoughts);
			bool result = Thought_Situational.tmpThoughts.Any((Thought x) => x.def == this.def);
			Thought_Situational.tmpThoughts.Clear();
			return result;
		}

		protected virtual ThoughtState CurrentStateInternal()
		{
			return this.def.Worker.CurrentState(this.pawn);
		}
	}
}
