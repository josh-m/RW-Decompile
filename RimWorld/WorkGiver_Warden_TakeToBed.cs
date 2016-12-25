using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_TakeToBed : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			Room room = pawn2.GetRoom();
			if (!pawn2.Downed && pawn.CanReserve(pawn2, 1))
			{
				bool flag = pawn2.ownership.OwnedBed != null && RoomQuery.RoomAt(pawn2.ownership.OwnedBed.Position) != RoomQuery.RoomAt(pawn2.Position);
				bool flag2 = false;
				if (!flag && room != null && !room.TouchesMapEdge)
				{
					foreach (Building_Bed current in room.ContainedBeds)
					{
						if (current.ForPrisoners && (!current.owners.Any<Pawn>() || current.owners.Contains(pawn2)) && (current.AnyUnoccupiedSleepingSlot || (pawn2.InBed() && pawn2.CurrentBed() == current)) && (!current.Medical || (pawn2.health.PrefersMedicalRest && pawn2.health.ShouldEverReceiveMedicalCare)))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag || !flag2)
				{
					Building_Bed building_Bed = RestUtility.FindBedFor(pawn2, pawn, true, false, false);
					if (building_Bed != null)
					{
						if (building_Bed.GetRoom() != room)
						{
							return new Job(JobDefOf.EscortPrisonerToBed, pawn2, building_Bed)
							{
								maxNumToCarry = 1
							};
						}
						Log.Error(string.Concat(new object[]
						{
							pawn,
							" tried to escort prisoner ",
							pawn2,
							" to bed at ",
							building_Bed.Position,
							" which is in the prisoner's room already."
						}));
					}
				}
			}
			if (pawn2.Downed && pawn2.health.NeedsMedicalRest && !pawn2.InBed() && pawn.CanReserve(pawn2, 1))
			{
				Building_Bed building_Bed2 = RestUtility.FindBedFor(pawn2, pawn, true, true, false);
				if (building_Bed2 != null && pawn2.CanReserve(building_Bed2, building_Bed2.SleepingSlotsCount))
				{
					return new Job(JobDefOf.TakeWoundedPrisonerToBed, pawn2, building_Bed2)
					{
						maxNumToCarry = 1
					};
				}
			}
			return null;
		}
	}
}
