using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PrisonBarracksImpressiveness : ThoughtWorker_RoomImpressiveness
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtState result = base.CurrentStateInternal(p);
			if (result.Active && p.IsPrisoner && p.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.PrisonBarracks)
			{
				return result;
			}
			return ThoughtState.Inactive;
		}
	}
}
