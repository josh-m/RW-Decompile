using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_FeedPatient : WorkGiver_Scanner
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
				return PathEndMode.Touch;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || pawn2 == pawn)
			{
				return false;
			}
			if (pawn2.needs.food == null || pawn2.needs.food.CurLevelPercentage > pawn2.needs.food.PercentageThreshHungry + 0.02f)
			{
				return false;
			}
			if (!FeedPatientUtility.ShouldBeFed(pawn2))
			{
				return false;
			}
			if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1))
			{
				return false;
			}
			Thing thing;
			ThingDef thingDef;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false, true, false, true))
			{
				JobFailReason.Is("NoFood".Translate());
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = (Pawn)t;
			Thing t2;
			ThingDef def;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out t2, out def, false, true, false, true))
			{
				return new Job(JobDefOf.FeedPatient)
				{
					targetA = t2,
					targetB = pawn2,
					count = FoodUtility.WillIngestStackCountOf(pawn2, def)
				};
			}
			return null;
		}
	}
}
