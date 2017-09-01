using System;
using System.Collections.Generic;
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
			ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Thing thing = innerContainer[i];
				if (thing.def.ingestible != null && thing.def.ingestible.nutrition > 0.3f && thing.def.ingestible.preferability >= FoodPreferability.MealAwful && pawn.RaceProps.CanEverEat(thing))
				{
					return null;
				}
			}
			if (pawn.Map.resourceCounter.TotalHumanEdibleNutrition < (float)pawn.Map.mapPawns.ColonistsSpawnedCount * 1.5f)
			{
				return null;
			}
			Thing thing2 = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 20f, delegate(Thing t)
			{
				if (t.def.category != ThingCategory.Item || t.def.ingestible == null || t.def.ingestible.nutrition < 0.3f || t.IsForbidden(pawn) || t is Corpse || !pawn.CanReserve(t, 1, -1, null, false) || !t.IsSociallyProper(pawn) || !pawn.RaceProps.CanEverEat(t))
				{
					return false;
				}
				List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(pawn, t);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].stages[0].baseMoodEffect < 0f)
					{
						return false;
					}
				}
				return true;
			}, (Thing x) => FoodUtility.FoodSourceOptimality(pawn, x, 0f, false), 24, 30);
			if (thing2 == null)
			{
				return null;
			}
			return new Job(JobDefOf.TakeInventory, thing2)
			{
				count = 1
			};
		}
	}
}
