using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_HospitalPatientRoomStats : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.InBed())
			{
				return ThoughtState.Inactive;
			}
			if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return ThoughtState.Inactive;
			}
			Building_Bed building_Bed = p.CurrentBed();
			Room room = p.GetRoom();
			if (!building_Bed.Medical || room == null || room.Role != RoomRoleDefOf.Hospital)
			{
				return ThoughtState.Inactive;
			}
			int scoreStageIndex = RoomStatDefOf.Beauty.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Beauty));
			if (this.def.stages[scoreStageIndex] != null)
			{
				return ThoughtState.ActiveAtStage(scoreStageIndex);
			}
			return ThoughtState.Inactive;
		}
	}
}
