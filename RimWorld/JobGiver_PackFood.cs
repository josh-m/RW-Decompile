using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PackFood : ThinkNode_JobGiver
	{
		private const float MinMealNutrition = 0.3f;

		private const float MinNutritionPerColonistToDo = 1.5f;

		public const FoodPreferability MinFoodPreferability = FoodPreferability.MealAwful;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.inventory == null)
			{
				return null;
			}
			ThingContainer container = pawn.inventory.container;
			for (int i = 0; i < container.Count; i++)
			{
				Thing thing = container[i];
				if (thing.def.ingestible != null && thing.def.ingestible.nutrition > 0.3f && thing.def.ingestible.preferability >= FoodPreferability.MealAwful && pawn.RaceProps.CanEverEat(thing))
				{
					return null;
				}
			}
			if (Find.ResourceCounter.TotalHumanEdibleNutrition < (float)Find.MapPawns.ColonistsSpawnedCount * 1.5f)
			{
				return null;
			}
			Thing thing2 = GenClosest.ClosestThingReachable(pawn.Position, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 20f, (Thing t) => t.def.category == ThingCategory.Item && t.def.ingestible != null && t.def.ingestible.nutrition > 0.3f && !t.IsForbidden(pawn) && t.def.category == ThingCategory.Item && !(t is Corpse) && pawn.CanReserve(t, 1) && t.IsSociallyProper(pawn) && pawn.RaceProps.CanEverEat(t), null, 9, false);
			if (thing2 == null)
			{
				return null;
			}
			return new Job(JobDefOf.TakeInventory, thing2)
			{
				maxNumToCarry = 1
			};
		}
	}
}
