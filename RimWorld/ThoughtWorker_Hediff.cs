using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Hediff : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(this.def.hediff);
			if (firstHediffOfDef == null || firstHediffOfDef.def.stages == null)
			{
				return ThoughtState.Inactive;
			}
			int stageIndex = Mathf.Min(new int[]
			{
				firstHediffOfDef.CurStageIndex,
				firstHediffOfDef.def.stages.Count - 1,
				this.def.stages.Count - 1
			});
			return ThoughtState.ActiveAtStage(stageIndex);
		}
	}
}
