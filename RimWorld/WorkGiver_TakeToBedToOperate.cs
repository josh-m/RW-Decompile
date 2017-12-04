using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_TakeToBedToOperate : WorkGiver_TakeToBed
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 != null && pawn2 != pawn && !pawn2.InBed() && pawn2.RaceProps.IsFlesh && HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn2))
			{
				LocalTargetInfo target = pawn2;
				if (pawn.CanReserve(target, 1, -1, null, forced) && (!pawn2.InMentalState || !pawn2.MentalStateDef.IsAggro))
				{
					if (!pawn2.Downed)
					{
						if (pawn2.IsColonist)
						{
							return false;
						}
						if (!pawn2.IsPrisonerOfColony && pawn2.Faction != Faction.OfPlayer)
						{
							return false;
						}
						if (pawn2.guest != null && pawn2.guest.Released)
						{
							return false;
						}
					}
					Building_Bed building_Bed = base.FindBed(pawn, pawn2);
					return building_Bed != null && pawn2.CanReserve(building_Bed, building_Bed.SleepingSlotsCount, -1, null, false);
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			Building_Bed t2 = base.FindBed(pawn, pawn2);
			return new Job(JobDefOf.TakeToBedToOperate, pawn2, t2)
			{
				count = 1
			};
		}
	}
}
