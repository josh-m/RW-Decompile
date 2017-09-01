using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public static class GenRecipe
	{
		[DebuggerHidden]
		public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient)
		{
			float efficiency;
			if (recipeDef.efficiencyStat == null)
			{
				efficiency = 1f;
			}
			else
			{
				efficiency = worker.GetStatValue(recipeDef.efficiencyStat, true);
			}
			if (recipeDef.products != null)
			{
				for (int i = 0; i < recipeDef.products.Count; i++)
				{
					ThingCountClass prod = recipeDef.products[i];
					ThingDef stuffDef;
					if (prod.thingDef.MadeFromStuff)
					{
						stuffDef = dominantIngredient.def;
					}
					else
					{
						stuffDef = null;
					}
					Thing product = ThingMaker.MakeThing(prod.thingDef, stuffDef);
					product.stackCount = Mathf.CeilToInt((float)prod.count * efficiency);
					if (dominantIngredient != null)
					{
						product.SetColor(dominantIngredient.DrawColor, false);
					}
					CompIngredients ingredientsComp = product.TryGetComp<CompIngredients>();
					if (ingredientsComp != null)
					{
						for (int j = 0; j < ingredients.Count; j++)
						{
							ingredientsComp.RegisterIngredient(ingredients[j].def);
						}
					}
					CompFoodPoisonable foodPoisonable = product.TryGetComp<CompFoodPoisonable>();
					if (foodPoisonable != null)
					{
						float poisonChance = worker.GetStatValue(StatDefOf.FoodPoisonChance, true);
						Room room = worker.GetRoom(RegionType.Set_Passable);
						if (room != null)
						{
							poisonChance *= room.GetStat(RoomStatDefOf.FoodPoisonChanceFactor);
						}
						if (Rand.Value < poisonChance)
						{
							foodPoisonable.PoisonPercent = 1f;
						}
					}
					yield return GenRecipe.PostProcessProduct(product, recipeDef, worker);
				}
			}
			if (recipeDef.specialProducts != null)
			{
				for (int k = 0; k < recipeDef.specialProducts.Count; k++)
				{
					for (int l = 0; l < ingredients.Count; l++)
					{
						Thing ing = ingredients[l];
						SpecialProductType specialProductType = recipeDef.specialProducts[k];
						if (specialProductType != SpecialProductType.Butchery)
						{
							if (specialProductType == SpecialProductType.Smelted)
							{
								foreach (Thing product2 in ing.SmeltProducts(efficiency))
								{
									yield return GenRecipe.PostProcessProduct(product2, recipeDef, worker);
								}
							}
						}
						else
						{
							foreach (Thing product3 in ing.ButcherProducts(worker, efficiency))
							{
								yield return GenRecipe.PostProcessProduct(product3, recipeDef, worker);
							}
						}
					}
				}
			}
		}

		private static Thing PostProcessProduct(Thing product, RecipeDef recipeDef, Pawn worker)
		{
			CompQuality compQuality = product.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				if (recipeDef.workSkill == null)
				{
					Log.Error(recipeDef + " needs workSkill because it creates a product with a quality.");
				}
				int level = worker.skills.GetSkill(recipeDef.workSkill).Level;
				compQuality.SetQuality(QualityUtility.RandomCreationQuality(level), ArtGenerationContext.Colony);
			}
			CompArt compArt = product.TryGetComp<CompArt>();
			if (compArt != null)
			{
				compArt.JustCreatedBy(worker);
				if (compQuality.Quality >= QualityCategory.Excellent)
				{
					TaleRecorder.RecordTale(TaleDefOf.CraftedArt, new object[]
					{
						worker,
						product
					});
				}
			}
			if (product.def.Minifiable)
			{
				product = product.MakeMinified();
			}
			return product;
		}
	}
}
