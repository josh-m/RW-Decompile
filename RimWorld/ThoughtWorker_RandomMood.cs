using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_RandomMood : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			switch ((p.GetHashCode() ^ (GenLocalDate.DayOfYear(p) + GenLocalDate.Year(p) * 60) * 391) % 10)
			{
			case 0:
				return ThoughtState.ActiveAtStage(0);
			case 1:
				return ThoughtState.ActiveAtStage(1);
			case 2:
				return ThoughtState.ActiveAtStage(1);
			case 3:
				return ThoughtState.ActiveAtStage(1);
			case 4:
				return ThoughtState.Inactive;
			case 5:
				return ThoughtState.Inactive;
			case 6:
				return ThoughtState.ActiveAtStage(2);
			case 7:
				return ThoughtState.ActiveAtStage(2);
			case 8:
				return ThoughtState.ActiveAtStage(2);
			case 9:
				return ThoughtState.ActiveAtStage(3);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
