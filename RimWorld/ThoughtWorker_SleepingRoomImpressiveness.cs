using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_SleepingRoomImpressiveness : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.story.traits.HasTrait(TraitDefOf.Ascetic))
			{
				return ThoughtState.Inactive;
			}
			if (p.ownership.OwnedBed == null)
			{
				return ThoughtState.Inactive;
			}
			Room room = p.ownership.OwnedBed.GetRoom();
			if (room == null)
			{
				return ThoughtState.Inactive;
			}
			int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
			if (this.def.stages[scoreStageIndex] != null)
			{
				return ThoughtState.ActiveAtStage(scoreStageIndex);
			}
			return ThoughtState.Inactive;
		}
	}
}
