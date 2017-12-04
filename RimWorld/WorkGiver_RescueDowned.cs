using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_RescueDowned : WorkGiver_TakeToBed
	{
		private const float MinDistFromEnemy = 40f;

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 != null && pawn2.Downed && pawn2.Faction == pawn.Faction && !pawn2.InBed())
			{
				LocalTargetInfo target = pawn2;
				if (pawn.CanReserve(target, 1, -1, null, forced) && !GenAI.EnemyIsNear(pawn2, 40f))
				{
					Thing thing = base.FindBed(pawn, pawn2);
					return thing != null && pawn2.CanReserve(thing, 1, -1, null, false);
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			Thing t2 = base.FindBed(pawn, pawn2);
			return new Job(JobDefOf.Rescue, pawn2, t2)
			{
				count = 1
			};
		}
	}
}
