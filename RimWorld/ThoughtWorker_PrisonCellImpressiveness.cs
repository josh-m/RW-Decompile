using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PrisonCellImpressiveness : ThoughtWorker_RoomImpressiveness
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtState result = base.CurrentStateInternal(p);
			if (result.Active && p.IsPrisoner && p.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.PrisonCell)
			{
				return result;
			}
			return ThoughtState.Inactive;
		}
	}
}
