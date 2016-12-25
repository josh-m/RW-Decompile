using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Cramped : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.space != null)
			{
				switch (p.needs.space.CurCategory)
				{
				case SpaceCategory.VeryCramped:
					return ThoughtState.ActiveAtStage(1);
				case SpaceCategory.Cramped:
					return ThoughtState.ActiveAtStage(0);
				case SpaceCategory.Normal:
					return ThoughtState.Inactive;
				}
			}
			return ThoughtState.Inactive;
		}
	}
}
