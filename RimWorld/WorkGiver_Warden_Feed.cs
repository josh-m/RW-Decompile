using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_Feed : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (!base.ShouldTakeCareOfPrisoner(pawn, t))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (!WardenFeedUtility.ShouldBeFed(pawn2))
			{
				return null;
			}
			if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
			{
				return null;
			}
			Thing t2;
			ThingDef def;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out t2, out def, false, true, false, false))
			{
				JobFailReason.Is("NoFood".Translate());
				return null;
			}
			return new Job(JobDefOf.FeedPatient, t2, pawn2)
			{
				count = FoodUtility.WillIngestStackCountOf(pawn2, def)
			};
		}
	}
}
