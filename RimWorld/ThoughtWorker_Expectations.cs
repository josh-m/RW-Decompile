using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Expectations : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return ThoughtState.Inactive;
			}
			if (p.Faction != Faction.OfPlayer)
			{
				return ThoughtState.ActiveAtStage(3);
			}
			float wealthTotal = Find.StoryWatcher.watcherWealth.WealthTotal;
			if (wealthTotal < 10000f)
			{
				return ThoughtState.ActiveAtStage(3);
			}
			if (wealthTotal < 50000f)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			if (wealthTotal < 150000f)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (wealthTotal < 300000f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			return ThoughtState.Inactive;
		}
	}
}
