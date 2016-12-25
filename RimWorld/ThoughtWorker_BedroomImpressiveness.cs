using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_BedroomImpressiveness : ThoughtWorker_SleepingRoomImpressiveness
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtState result = base.CurrentStateInternal(p);
			if (result.Active && p.ownership.OwnedBed.GetRoom().Role == RoomRoleDefOf.Bedroom)
			{
				return result;
			}
			return ThoughtState.Inactive;
		}
	}
}
