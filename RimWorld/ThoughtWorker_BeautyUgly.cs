using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_BeautyUgly : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.beauty == null)
			{
				return ThoughtState.Inactive;
			}
			switch (p.needs.beauty.CurCategory)
			{
			case BeautyCategory.Hideous:
				return ThoughtState.ActiveAtStage(2);
			case BeautyCategory.VeryUgly:
				return ThoughtState.ActiveAtStage(1);
			case BeautyCategory.Ugly:
				return ThoughtState.ActiveAtStage(0);
			case BeautyCategory.Neutral:
				return ThoughtState.Inactive;
			default:
				return ThoughtState.Inactive;
			}
		}
	}
}
