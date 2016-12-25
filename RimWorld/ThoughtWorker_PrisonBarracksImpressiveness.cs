using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PrisonBarracksImpressiveness : ThoughtWorker_SleepingRoomImpressiveness
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtState result = base.CurrentStateInternal(p);
			if (result.Active && p.ownership.OwnedBed.GetRoom().Role == RoomRoleDefOf.PrisonBarracks)
			{
				return result;
			}
			return ThoughtState.Inactive;
		}
	}
}
