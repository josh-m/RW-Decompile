using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_BeautyPretty : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.beauty == null)
			{
				return ThoughtState.Inactive;
			}
			switch (p.needs.beauty.CurCategory)
			{
			case BeautyCategory.Neutral:
				return ThoughtState.Inactive;
			case BeautyCategory.Pretty:
				return ThoughtState.ActiveAtStage(0);
			case BeautyCategory.VeryPretty:
				return ThoughtState.ActiveAtStage(1);
			case BeautyCategory.Beautiful:
				return ThoughtState.ActiveAtStage(2);
			default:
				return ThoughtState.Inactive;
			}
		}
	}
}
