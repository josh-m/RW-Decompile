using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Bed
	{
		public static Toil GotoBed(TargetIndex bedIndex)
		{
			Toil gotoBed = new Toil();
			gotoBed.initAction = delegate
			{
				Pawn actor = gotoBed.actor;
				Building_Bed bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
				IntVec3 bedSleepingSlotPosFor = RestUtility.GetBedSleepingSlotPosFor(actor, bed);
				if (actor.Position == bedSleepingSlotPosFor)
				{
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else
				{
					actor.pather.StartPath(RestUtility.GetBedSleepingSlotPosFor(actor, bed), PathEndMode.OnCell);
				}
			};
			gotoBed.tickAction = delegate
			{
				Pawn actor = gotoBed.actor;
				Building_Bed building_Bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
				Pawn curOccupantAt = building_Bed.GetCurOccupantAt(actor.pather.Destination.Cell);
				if (curOccupantAt != null && curOccupantAt != actor)
				{
					actor.pather.StartPath(RestUtility.GetBedSleepingSlotPosFor(actor, building_Bed), PathEndMode.OnCell);
				}
			};
			gotoBed.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoBed.FailOnBedNoLongerUsable(bedIndex);
			return gotoBed;
		}

		public static Toil ClaimBedIfNonMedical(TargetIndex ind, TargetIndex claimantIndex = TargetIndex.None)
		{
			Toil claim = new Toil();
			claim.initAction = delegate
			{
				Pawn actor = claim.GetActor();
				Pawn pawn = (claimantIndex != TargetIndex.None) ? ((Pawn)actor.CurJob.GetTarget(claimantIndex).Thing) : actor;
				if (pawn.ownership != null)
				{
					pawn.ownership.ClaimBedIfNonMedical((Building_Bed)actor.CurJob.GetTarget(ind).Thing);
				}
			};
			claim.FailOnDespawnedOrNull(ind);
			return claim;
		}

		public static T FailOnNonMedicalBedNotOwned<T>(this T f, TargetIndex bedIndex, TargetIndex claimantIndex = TargetIndex.None) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Pawn pawn = (claimantIndex != TargetIndex.None) ? ((Pawn)actor.CurJob.GetTarget(claimantIndex).Thing) : actor;
				if (pawn.ownership != null)
				{
					Building_Bed building_Bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
					if (building_Bed.Medical)
					{
						if ((!pawn.InBed() || pawn.CurrentBed() != building_Bed) && !building_Bed.AnyUnoccupiedSleepingSlot)
						{
							return JobCondition.Incompletable;
						}
					}
					else
					{
						if (!building_Bed.owners.Contains(pawn))
						{
							return JobCondition.Incompletable;
						}
						if (pawn.InBed() && pawn.CurrentBed() == building_Bed)
						{
							int curOccupantSlotIndex = building_Bed.GetCurOccupantSlotIndex(pawn);
							if (curOccupantSlotIndex >= building_Bed.owners.Count || building_Bed.owners[curOccupantSlotIndex] != pawn)
							{
								return JobCondition.Incompletable;
							}
						}
					}
				}
				return JobCondition.Ongoing;
			});
			return f;
		}

		public static void FailOnBedNoLongerUsable(this Toil toil, TargetIndex bedIndex)
		{
			toil.FailOnDespawnedOrNull(bedIndex);
			toil.FailOn(() => ((Building_Bed)toil.actor.CurJob.GetTarget(bedIndex).Thing).IsBurning());
			toil.FailOn(() => ((Building_Bed)toil.actor.CurJob.GetTarget(bedIndex).Thing).ForPrisoners != toil.actor.IsPrisoner);
			toil.FailOnNonMedicalBedNotOwned(bedIndex, TargetIndex.None);
			toil.FailOn(() => !HealthAIUtility.ShouldSeekMedicalRest(toil.actor) && !HealthAIUtility.ShouldSeekMedicalRestUrgent(toil.actor) && ((Building_Bed)toil.actor.CurJob.GetTarget(bedIndex).Thing).Medical);
			toil.FailOn(() => toil.actor.IsColonist && !toil.actor.CurJob.ignoreForbidden && !toil.actor.Downed && toil.actor.CurJob.GetTarget(bedIndex).Thing.IsForbidden(toil.actor));
		}
	}
}
