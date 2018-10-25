using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_InteractAnimal : WorkGiver_Scanner
	{
		protected static string NoUsableFoodTrans;

		protected static string AnimalInteractedTooRecentlyTrans;

		private static string CantInteractAnimalDownedTrans;

		private static string CantInteractAnimalAsleepTrans;

		private static string CantInteractAnimalBusyTrans;

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public static void ResetStaticData()
		{
			WorkGiver_InteractAnimal.NoUsableFoodTrans = "NoUsableFood".Translate();
			WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans = "AnimalInteractedTooRecently".Translate();
			WorkGiver_InteractAnimal.CantInteractAnimalDownedTrans = "CantInteractAnimalDowned".Translate();
			WorkGiver_InteractAnimal.CantInteractAnimalAsleepTrans = "CantInteractAnimalAsleep".Translate();
			WorkGiver_InteractAnimal.CantInteractAnimalBusyTrans = "CantInteractAnimalBusy".Translate();
		}

		protected virtual bool CanInteractWithAnimal(Pawn pawn, Pawn animal, bool forced)
		{
			LocalTargetInfo target = animal;
			if (!pawn.CanReserve(target, 1, -1, null, forced))
			{
				return false;
			}
			if (animal.Downed)
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.CantInteractAnimalDownedTrans, null);
				return false;
			}
			if (!animal.Awake())
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.CantInteractAnimalAsleepTrans, null);
				return false;
			}
			if (!animal.CanCasuallyInteractNow(false))
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.CantInteractAnimalBusyTrans, null);
				return false;
			}
			int num = TrainableUtility.MinimumHandlingSkill(animal);
			if (num > pawn.skills.GetSkill(SkillDefOf.Animals).Level)
			{
				JobFailReason.Is("AnimalsSkillTooLow".Translate(num), null);
				return false;
			}
			return true;
		}

		protected bool HasFoodToInteractAnimal(Pawn pawn, Pawn tamee)
		{
			ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
			int num = 0;
			float num2 = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);
			float num3 = 0f;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Thing thing = innerContainer[i];
				if (tamee.WillEat(thing, pawn) && thing.def.ingestible.preferability <= FoodPreferability.RawTasty && !thing.def.IsDrug)
				{
					for (int j = 0; j < thing.stackCount; j++)
					{
						num3 += thing.GetStatValue(StatDefOf.Nutrition, true);
						if (num3 >= num2)
						{
							num++;
							num3 = 0f;
						}
						if (num >= 2)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		protected Job TakeFoodForAnimalInteractJob(Pawn pawn, Pawn tamee)
		{
			float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
			ThingDef foodDef;
			Thing thing = FoodUtility.BestFoodSourceOnMap(pawn, tamee, false, out foodDef, FoodPreferability.RawTasty, false, false, false, false, false, false, false, false, false);
			if (thing == null)
			{
				return null;
			}
			float nutrition = FoodUtility.GetNutrition(thing, foodDef);
			return new Job(JobDefOf.TakeInventory, thing)
			{
				count = Mathf.CeilToInt(num / nutrition)
			};
		}
	}
}
