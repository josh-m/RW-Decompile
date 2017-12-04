using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	internal static class DataAnalysisTableMaker
	{
		public static void DoTable_PopulationIntent()
		{
			Find.Storyteller.intenderPopulation.DoTable_PopulationIntents();
		}

		public static void DoTable_PopAdjRecruitDifficulty()
		{
			PawnUtility.DoTable_PopIntentRecruitDifficulty();
		}

		public static void DoTable_ManhunterResults()
		{
			ManhunterPackIncidentUtility.DoTable_ManhunterResults();
		}

		public static void DoTable_DrugEconomy()
		{
			Func<ThingDef, string> ingredients = delegate(ThingDef d)
			{
				if (d.costList == null)
				{
					return "-";
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ThingCountClass current in d.costList)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
					string text = (!DataAnalysisTableMaker.RequiresBuying(current.thingDef)) ? string.Empty : "*";
					stringBuilder.Append(string.Concat(new object[]
					{
						current.thingDef.defName,
						text,
						" x",
						current.count
					}));
				}
				return stringBuilder.ToString().TrimEndNewlines();
			};
			Func<ThingDef, float> workAmount = delegate(ThingDef d)
			{
				if (d.recipeMaker == null)
				{
					return -1f;
				}
				if (d.recipeMaker.workAmount >= 0)
				{
					return (float)d.recipeMaker.workAmount;
				}
				return Mathf.Max(d.GetStatValueAbstract(StatDefOf.WorkToMake, null), d.GetStatValueAbstract(StatDefOf.WorkToBuild, null));
			};
			Func<ThingDef, float> realIngredientCost = (ThingDef d) => DataAnalysisTableMaker.CostToMake(d, true);
			Func<ThingDef, float> realSellPrice = (ThingDef d) => d.BaseMarketValue * 0.5f;
			Func<ThingDef, float> realBuyPrice = (ThingDef d) => d.BaseMarketValue * 1.5f;
			IEnumerable<ThingDef> arg_1BB_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsWithinCategory(ThingCategoryDefOf.Medicine) || d.IsWithinCategory(ThingCategoryDefOf.Drugs)
			select d;
			TableDataGetter<ThingDef>[] expr_E2 = new TableDataGetter<ThingDef>[8];
			expr_E2[0] = new TableDataGetter<ThingDef>("name", (ThingDef d) => d.defName);
			expr_E2[1] = new TableDataGetter<ThingDef>("ingredients", (ThingDef d) => ingredients(d));
			expr_E2[2] = new TableDataGetter<ThingDef>("work amount", (ThingDef d) => workAmount(d).ToString("F0"));
			expr_E2[3] = new TableDataGetter<ThingDef>("real ingredient cost", (ThingDef d) => realIngredientCost(d).ToString("F1"));
			expr_E2[4] = new TableDataGetter<ThingDef>("real sell price", (ThingDef d) => realSellPrice(d).ToString("F1"));
			expr_E2[5] = new TableDataGetter<ThingDef>("real profit per item", (ThingDef d) => (realSellPrice(d) - realIngredientCost(d)).ToString("F1"));
			expr_E2[6] = new TableDataGetter<ThingDef>("real profit per day's work", (ThingDef d) => ((realSellPrice(d) - realIngredientCost(d)) / workAmount(d) * 30000f).ToString("F1"));
			expr_E2[7] = new TableDataGetter<ThingDef>("real buy price", (ThingDef d) => realBuyPrice(d).ToString("F1"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_1BB_0, expr_E2);
		}

		public static void DoTable_WoolEconomy()
		{
			IEnumerable<ThingDef> arg_129_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.IsFlesh && d.GetCompProperties<CompProperties_Shearable>() != null
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[6];
			expr_2D[0] = new TableDataGetter<ThingDef>("animal", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("woolDef", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolDef.defName);
			expr_2D[2] = new TableDataGetter<ThingDef>("woolAmount", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolAmount.ToString());
			expr_2D[3] = new TableDataGetter<ThingDef>("woolValue", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolDef.BaseMarketValue.ToString("F2"));
			expr_2D[4] = new TableDataGetter<ThingDef>("shear interval", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().shearIntervalDays.ToString("F1"));
			expr_2D[5] = new TableDataGetter<ThingDef>("value per year", delegate(ThingDef d)
			{
				CompProperties_Shearable compProperties = d.GetCompProperties<CompProperties_Shearable>();
				return (compProperties.woolDef.BaseMarketValue * (float)compProperties.woolAmount * (60f / (float)compProperties.shearIntervalDays)).ToString("F0");
			});
			DebugTables.MakeTablesDialog<ThingDef>(arg_129_0, expr_2D);
		}

		public static void DoTable_AnimalGrowthEconomy()
		{
			Func<ThingDef, float> gestDays = delegate(ThingDef d)
			{
				if (d.HasComp(typeof(CompEggLayer)))
				{
					CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
					return compProperties.eggLayIntervalDays / compProperties.eggCountRange.Average;
				}
				return d.race.gestationPeriodDays;
			};
			Func<ThingDef, float> nutritionToGestate = delegate(ThingDef d)
			{
				float num = 0f;
				LifeStageAge lifeStageAge = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1];
				return num + gestDays(d) * lifeStageAge.def.hungerRateFactor * d.race.baseHungerRate;
			};
			Func<ThingDef, float> babyMeatNut = delegate(ThingDef d)
			{
				LifeStageAge lifeStageAge = d.race.lifeStageAges[0];
				return d.GetStatValueAbstract(StatDefOf.MeatAmount, null) * 0.05f * lifeStageAge.def.bodySizeFactor;
			};
			Func<ThingDef, float> babyMeatNutPerInput = (ThingDef d) => babyMeatNut(d) / nutritionToGestate(d);
			Func<ThingDef, float> nutritionToAdulthood = delegate(ThingDef d)
			{
				float num = 0f;
				num += nutritionToGestate(d);
				for (int i = 1; i < d.race.lifeStageAges.Count; i++)
				{
					LifeStageAge lifeStageAge = d.race.lifeStageAges[i];
					float num2 = lifeStageAge.minAge - d.race.lifeStageAges[i - 1].minAge;
					float num3 = num2 * 60f;
					num += num3 * lifeStageAge.def.hungerRateFactor * d.race.baseHungerRate;
				}
				return num;
			};
			Func<ThingDef, float> adultMeatNutPerInput = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MeatAmount, null) * 0.05f / nutritionToAdulthood(d);
			Func<ThingDef, float> bestMeatPerInput = delegate(ThingDef d)
			{
				float a = babyMeatNutPerInput(d);
				float b = adultMeatNutPerInput(d);
				return Mathf.Max(a, b);
			};
			Func<ThingDef, string> eggNut = delegate(ThingDef d)
			{
				CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
				if (compProperties == null)
				{
					return string.Empty;
				}
				return compProperties.eggFertilizedDef.ingestible.nutrition.ToString("F2");
			};
			IEnumerable<ThingDef> arg_37D_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.IsFlesh
			orderby bestMeatPerInput(d) descending
			select d;
			TableDataGetter<ThingDef>[] expr_108 = new TableDataGetter<ThingDef>[18];
			expr_108[0] = new TableDataGetter<ThingDef>("animal", (ThingDef d) => d.defName);
			expr_108[1] = new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => d.race.baseHungerRate.ToString("F2"));
			expr_108[2] = new TableDataGetter<ThingDef>("gestDays", (ThingDef d) => gestDays(d).ToString("F2"));
			expr_108[3] = new TableDataGetter<ThingDef>("herbiv", (ThingDef d) => ((d.race.foodType & FoodTypeFlags.Plant) == FoodTypeFlags.None) ? string.Empty : "Y");
			expr_108[4] = new TableDataGetter<ThingDef>("eggs", (ThingDef d) => (!d.HasComp(typeof(CompEggLayer))) ? string.Empty : d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.ToString());
			expr_108[5] = new TableDataGetter<ThingDef>("|", (ThingDef d) => "|");
			expr_108[6] = new TableDataGetter<ThingDef>("bodySize", (ThingDef d) => d.race.baseBodySize.ToString("F2"));
			expr_108[7] = new TableDataGetter<ThingDef>("age Adult", (ThingDef d) => d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge.ToString("F2"));
			expr_108[8] = new TableDataGetter<ThingDef>("nutrition to adulthood", (ThingDef d) => nutritionToAdulthood(d).ToString("F2"));
			expr_108[9] = new TableDataGetter<ThingDef>("adult meat-nut", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.MeatAmount, null) * 0.05f).ToString("F2"));
			expr_108[10] = new TableDataGetter<ThingDef>("adult meat-nut / input-nut", (ThingDef d) => adultMeatNutPerInput(d).ToString("F3"));
			expr_108[11] = new TableDataGetter<ThingDef>("|", (ThingDef d) => "|");
			expr_108[12] = new TableDataGetter<ThingDef>("baby size", (ThingDef d) => (d.race.lifeStageAges[0].def.bodySizeFactor * d.race.baseBodySize).ToString("F2"));
			expr_108[13] = new TableDataGetter<ThingDef>("nutrition to gestate", (ThingDef d) => nutritionToGestate(d).ToString("F2"));
			expr_108[14] = new TableDataGetter<ThingDef>("egg nut", (ThingDef d) => eggNut(d));
			expr_108[15] = new TableDataGetter<ThingDef>("baby meat-nut", (ThingDef d) => babyMeatNut(d).ToString("F2"));
			expr_108[16] = new TableDataGetter<ThingDef>("baby meat-nut / input-nut", (ThingDef d) => babyMeatNutPerInput(d).ToString("F2"));
			expr_108[17] = new TableDataGetter<ThingDef>("baby wins", (ThingDef d) => (babyMeatNutPerInput(d) <= adultMeatNutPerInput(d)) ? string.Empty : "B");
			DebugTables.MakeTablesDialog<ThingDef>(arg_37D_0, expr_108);
		}

		public static void DoTable_CropEconomy()
		{
			Func<ThingDef, float> workCost = delegate(ThingDef d)
			{
				float num = 1.1f;
				num += d.plant.growDays * 1f;
				return num + (d.plant.sowWork + d.plant.harvestWork) * 0.00612f;
			};
			IEnumerable<ThingDef> arg_241_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Plant && d.plant.Harvestable && d.plant.Sowable
			orderby d.plant.IsTree
			select d;
			TableDataGetter<ThingDef>[] expr_79 = new TableDataGetter<ThingDef>[12];
			expr_79[0] = new TableDataGetter<ThingDef>("plant", (ThingDef d) => d.defName);
			expr_79[1] = new TableDataGetter<ThingDef>("product", (ThingDef d) => d.plant.harvestedThingDef.defName);
			expr_79[2] = new TableDataGetter<ThingDef>("grow time", (ThingDef d) => d.plant.growDays.ToString("F1"));
			expr_79[3] = new TableDataGetter<ThingDef>("work", (ThingDef d) => (d.plant.sowWork + d.plant.harvestWork).ToString("F0"));
			expr_79[4] = new TableDataGetter<ThingDef>("harvestCount", (ThingDef d) => d.plant.harvestYield.ToString("F1"));
			expr_79[5] = new TableDataGetter<ThingDef>("work-cost per cycle", (ThingDef d) => workCost(d).ToString("F2"));
			expr_79[6] = new TableDataGetter<ThingDef>("work-cost per harvestCount", (ThingDef d) => (workCost(d) / d.plant.harvestYield).ToString("F2"));
			expr_79[7] = new TableDataGetter<ThingDef>("value each", (ThingDef d) => d.plant.harvestedThingDef.BaseMarketValue.ToString("F2"));
			expr_79[8] = new TableDataGetter<ThingDef>("harvestValueTotal", (ThingDef d) => (d.plant.harvestYield * d.plant.harvestedThingDef.BaseMarketValue).ToString("F2"));
			expr_79[9] = new TableDataGetter<ThingDef>("profit per growDay", (ThingDef d) => ((d.plant.harvestYield * d.plant.harvestedThingDef.BaseMarketValue - workCost(d)) / d.plant.growDays).ToString("F2"));
			expr_79[10] = new TableDataGetter<ThingDef>("nutrition per growDay", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? string.Empty : (d.plant.harvestYield * d.plant.harvestedThingDef.ingestible.nutrition / d.plant.growDays).ToString("F2"));
			expr_79[11] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? string.Empty : d.plant.harvestedThingDef.ingestible.nutrition.ToString("F2"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_241_0, expr_79);
		}

		public static void DoTable_ItemNutritions()
		{
			IEnumerable<ThingDef> arg_121_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && d.IsNutritionGivingIngestible
			orderby d.ingestible.nutrition
			select d;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[5];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.BaseMarketValue.ToString("F1"));
			expr_4F[2] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => d.ingestible.nutrition.ToString("F2"));
			expr_4F[3] = new TableDataGetter<ThingDef>("nutrition per value", (ThingDef d) => (d.ingestible.nutrition / d.BaseMarketValue).ToString("F3"));
			expr_4F[4] = new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, null).ToString("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_121_0, expr_4F);
		}

		public static void DoTable_AllNutritions()
		{
			IEnumerable<ThingDef> arg_81_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.ingestible != null
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[2];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => d.ingestible.nutrition.ToStringPercentEmptyZero("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_81_0, expr_2D);
		}

		public static void DoTable_ItemMarketValuesStackable()
		{
			DataAnalysisTableMaker.DoItemMarketValues(true);
		}

		public static void DoTable_ItemMarketValuesUnstackable()
		{
			DataAnalysisTableMaker.DoItemMarketValues(false);
		}

		private static void DoItemMarketValues(bool stackable)
		{
			Func<ThingDef, float> workAmountGetter = delegate(ThingDef d)
			{
				if (d.recipeMaker == null)
				{
					return -1f;
				}
				if (d.recipeMaker.workAmount >= 0)
				{
					return (float)d.recipeMaker.workAmount;
				}
				return Mathf.Max(d.GetStatValueAbstract(StatDefOf.WorkToMake, null), d.GetStatValueAbstract(StatDefOf.WorkToBuild, null));
			};
			IEnumerable<ThingDef> arg_148_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && d.BaseMarketValue > 0.01f && d.stackLimit > 1 == stackable
			orderby d.BaseMarketValue
			select d;
			TableDataGetter<ThingDef>[] expr_6E = new TableDataGetter<ThingDef>[6];
			expr_6E[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_6E[1] = new TableDataGetter<ThingDef>("base market value", (ThingDef d) => d.BaseMarketValue.ToString("F1"));
			expr_6E[2] = new TableDataGetter<ThingDef>("cost to make", (ThingDef d) => DataAnalysisTableMaker.CostToMakeString(d, false));
			expr_6E[3] = new TableDataGetter<ThingDef>("work to make", (ThingDef d) => (d.recipeMaker == null) ? "-" : workAmountGetter(d).ToString("F1"));
			expr_6E[4] = new TableDataGetter<ThingDef>("profit", (ThingDef d) => (d.BaseMarketValue - DataAnalysisTableMaker.CostToMake(d, false)).ToString("F1"));
			expr_6E[5] = new TableDataGetter<ThingDef>("profit rate", (ThingDef d) => (d.recipeMaker == null) ? "-" : ((d.BaseMarketValue - DataAnalysisTableMaker.CostToMake(d, false)) / workAmountGetter(d) * 10000f).ToString("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_148_0, expr_6E);
		}

		public static void DoTable_Stuffs()
		{
			Func<ThingDef, StatDef, string> workGetter = delegate(ThingDef d, StatDef stat)
			{
				if (d.stuffProps.statFactors == null)
				{
					return string.Empty;
				}
				StatModifier statModifier = d.stuffProps.statFactors.FirstOrDefault((StatModifier fa) => fa.stat == stat);
				if (statModifier == null)
				{
					return string.Empty;
				}
				return statModifier.value.ToString();
			};
			IEnumerable<ThingDef> arg_FE_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsStuff
			orderby d.BaseMarketValue
			select d;
			TableDataGetter<ThingDef>[] expr_78 = new TableDataGetter<ThingDef>[4];
			expr_78[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_78[1] = new TableDataGetter<ThingDef>("base market value", (ThingDef d) => d.BaseMarketValue.ToString("F1"));
			expr_78[2] = new TableDataGetter<ThingDef>("fac-WorkToMake", (ThingDef d) => workGetter(d, StatDefOf.WorkToMake));
			expr_78[3] = new TableDataGetter<ThingDef>("fac-WorkToBuild", (ThingDef d) => workGetter(d, StatDefOf.WorkToBuild));
			DebugTables.MakeTablesDialog<ThingDef>(arg_FE_0, expr_78);
		}

		public static void DoTable_ProductionRecipes()
		{
			Func<RecipeDef, float> trueWork = (RecipeDef d) => d.WorkAmountTotal(null);
			Func<RecipeDef, float> cheapestIngredientVal = delegate(RecipeDef d)
			{
				float num = 0f;
				foreach (IngredientCount current in d.ingredients)
				{
					num += current.filter.AllowedThingDefs.Min((ThingDef td) => td.BaseMarketValue) * current.GetBaseCount();
				}
				return num;
			};
			Func<RecipeDef, float> workVal = (RecipeDef d) => trueWork(d) * 0.01f;
			Func<RecipeDef, float> cheapestProductsVal = delegate(RecipeDef d)
			{
				ThingDef thingDef = d.ingredients.First<IngredientCount>().filter.AllowedThingDefs.MinBy((ThingDef td) => td.BaseMarketValue);
				float num = 0f;
				foreach (ThingCountClass current in d.products)
				{
					num += current.thingDef.GetStatValueAbstract(StatDefOf.MarketValue, (!current.thingDef.MadeFromStuff) ? null : thingDef) * (float)current.count;
				}
				return num;
			};
			IEnumerable<RecipeDef> arg_187_0 = from d in DefDatabase<RecipeDef>.AllDefs
			where !d.products.NullOrEmpty<ThingCountClass>() && !d.ingredients.NullOrEmpty<IngredientCount>()
			select d;
			TableDataGetter<RecipeDef>[] expr_AE = new TableDataGetter<RecipeDef>[8];
			expr_AE[0] = new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName);
			expr_AE[1] = new TableDataGetter<RecipeDef>("work", (RecipeDef d) => trueWork(d).ToString("F0"));
			expr_AE[2] = new TableDataGetter<RecipeDef>("cheapest ingredients value", (RecipeDef d) => cheapestIngredientVal(d).ToString("F1"));
			expr_AE[3] = new TableDataGetter<RecipeDef>("work value", (RecipeDef d) => workVal(d).ToString("F1"));
			expr_AE[4] = new TableDataGetter<RecipeDef>("cheapest products value", (RecipeDef d) => cheapestProductsVal(d).ToString("F1"));
			expr_AE[5] = new TableDataGetter<RecipeDef>("profit raw", (RecipeDef d) => (cheapestProductsVal(d) - cheapestIngredientVal(d)).ToString("F1"));
			expr_AE[6] = new TableDataGetter<RecipeDef>("profit with work", (RecipeDef d) => (cheapestProductsVal(d) - workVal(d) - cheapestIngredientVal(d)).ToString("F1"));
			expr_AE[7] = new TableDataGetter<RecipeDef>("profit per work day", (RecipeDef d) => ((cheapestProductsVal(d) - cheapestIngredientVal(d)) * 60000f / trueWork(d)).ToString("F0"));
			DebugTables.MakeTablesDialog<RecipeDef>(arg_187_0, expr_AE);
		}

		private static string CostToMakeString(ThingDef d, bool real = false)
		{
			if (d.recipeMaker == null)
			{
				return "-";
			}
			return DataAnalysisTableMaker.CostToMake(d, real).ToString("F1");
		}

		private static float CostToMake(ThingDef d, bool real = false)
		{
			if (d.recipeMaker == null)
			{
				return d.BaseMarketValue;
			}
			float num = 0f;
			if (d.costList != null)
			{
				foreach (ThingCountClass current in d.costList)
				{
					float num2 = 1f;
					if (real)
					{
						num2 = ((!DataAnalysisTableMaker.RequiresBuying(current.thingDef)) ? 0.5f : 1.5f);
					}
					num += (float)current.count * DataAnalysisTableMaker.CostToMake(current.thingDef, true) * num2;
				}
			}
			if (d.costStuffCount > 0)
			{
				ThingDef thingDef = GenStuff.DefaultStuffFor(d);
				num += (float)d.costStuffCount * thingDef.BaseMarketValue;
			}
			return num;
		}

		private static bool RequiresBuying(ThingDef def)
		{
			if (def.costList != null)
			{
				foreach (ThingCountClass current in def.costList)
				{
					if (DataAnalysisTableMaker.RequiresBuying(current.thingDef))
					{
						return true;
					}
				}
				return false;
			}
			return !DefDatabase<ThingDef>.AllDefs.Any((ThingDef d) => d.plant != null && d.plant.harvestedThingDef == def && d.plant.Sowable);
		}

		public static void DoTable_RacesBasics()
		{
			Func<PawnKindDef, float> dps = (PawnKindDef d) => DataAnalysisTableMaker.RaceMeleeDpsEstimate(d.race);
			Func<PawnKindDef, float> pointsGuess = delegate(PawnKindDef d)
			{
				float num = 15f;
				num += dps(d) * 10f;
				num *= Mathf.Lerp(1f, d.race.GetStatValueAbstract(StatDefOf.MoveSpeed, null) / 3f, 0.25f);
				num *= d.RaceProps.baseHealthScale;
				num *= GenMath.LerpDouble(0.25f, 1f, 1.65f, 1f, Mathf.Clamp(d.RaceProps.baseBodySize, 0.25f, 1f));
				return num * 0.76f;
			};
			Func<PawnKindDef, float> mktValGuess = delegate(PawnKindDef d)
			{
				float num = 18f;
				num += pointsGuess(d) * 2.7f;
				if (d.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.None)
				{
					num *= 0.5f;
				}
				else if (d.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.Simple)
				{
					num *= 0.8f;
				}
				else if (d.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.Intermediate)
				{
					num = num;
				}
				else
				{
					if (d.RaceProps.TrainableIntelligence != TrainableIntelligenceDefOf.Advanced)
					{
						throw new InvalidOperationException();
					}
					num += 250f;
				}
				num += d.RaceProps.baseBodySize * 80f;
				if (d.race.HasComp(typeof(CompMilkable)))
				{
					num += 125f;
				}
				if (d.race.HasComp(typeof(CompShearable)))
				{
					num += 90f;
				}
				if (d.race.HasComp(typeof(CompEggLayer)))
				{
					num += 90f;
				}
				num *= Mathf.Lerp(0.8f, 1.2f, d.RaceProps.wildness);
				return num * 0.75f;
			};
			IEnumerable<PawnKindDef> arg_345_0 = from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && !d.RaceProps.Humanlike
			select d;
			TableDataGetter<PawnKindDef>[] expr_7B = new TableDataGetter<PawnKindDef>[18];
			expr_7B[0] = new TableDataGetter<PawnKindDef>("defName", (PawnKindDef d) => d.defName);
			expr_7B[1] = new TableDataGetter<PawnKindDef>("points", (PawnKindDef d) => d.combatPower.ToString("F0"));
			expr_7B[2] = new TableDataGetter<PawnKindDef>("points guess", (PawnKindDef d) => pointsGuess(d).ToString("F0"));
			expr_7B[3] = new TableDataGetter<PawnKindDef>("mktval", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.MarketValue, null).ToString("F0"));
			expr_7B[4] = new TableDataGetter<PawnKindDef>("mktval guess", (PawnKindDef d) => mktValGuess(d).ToString("F0"));
			expr_7B[5] = new TableDataGetter<PawnKindDef>("healthScale", (PawnKindDef d) => d.RaceProps.baseHealthScale.ToString("F2"));
			expr_7B[6] = new TableDataGetter<PawnKindDef>("bodySize", (PawnKindDef d) => d.RaceProps.baseBodySize.ToString("F2"));
			expr_7B[7] = new TableDataGetter<PawnKindDef>("hunger rate", (PawnKindDef d) => d.RaceProps.baseHungerRate.ToString("F2"));
			expr_7B[8] = new TableDataGetter<PawnKindDef>("speed", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.MoveSpeed, null).ToString("F2"));
			expr_7B[9] = new TableDataGetter<PawnKindDef>("melee dps", (PawnKindDef d) => dps(d).ToString("F2"));
			expr_7B[10] = new TableDataGetter<PawnKindDef>("wildness", (PawnKindDef d) => d.RaceProps.wildness.ToStringPercent());
			expr_7B[11] = new TableDataGetter<PawnKindDef>("life expec.", (PawnKindDef d) => d.RaceProps.lifeExpectancy.ToString("F1"));
			expr_7B[12] = new TableDataGetter<PawnKindDef>("train-int", (PawnKindDef d) => d.RaceProps.TrainableIntelligence.label);
			expr_7B[13] = new TableDataGetter<PawnKindDef>("temps", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null).ToString("F0") + ".." + d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null).ToString("F0"));
			expr_7B[14] = new TableDataGetter<PawnKindDef>("mateMtb", (PawnKindDef d) => d.RaceProps.mateMtbHours.ToStringEmptyZero("F0"));
			expr_7B[15] = new TableDataGetter<PawnKindDef>("nuzzMtb", (PawnKindDef d) => d.RaceProps.nuzzleMtbHours.ToStringEmptyZero("F0"));
			expr_7B[16] = new TableDataGetter<PawnKindDef>("mhChDam", (PawnKindDef d) => d.RaceProps.manhunterOnDamageChance.ToStringPercentEmptyZero("F2"));
			expr_7B[17] = new TableDataGetter<PawnKindDef>("mhChTam", (PawnKindDef d) => d.RaceProps.manhunterOnTameFailChance.ToStringPercentEmptyZero("F2"));
			DebugTables.MakeTablesDialog<PawnKindDef>(arg_345_0, expr_7B);
		}

		private static float RaceMeleeDpsEstimate(ThingDef race)
		{
			if (race.Verbs.NullOrEmpty<VerbProperties>())
			{
				return 0f;
			}
			IEnumerable<VerbProperties> list = from v in race.Verbs
			where (float)v.meleeDamageBaseAmount > 0.001f
			select v;
			return list.AverageWeighted((VerbProperties v) => v.BaseMeleeSelectionWeight, (VerbProperties v) => (float)v.meleeDamageBaseAmount / (v.defaultCooldownTime + v.warmupTime));
		}

		public static void DoTable_RacesFoodConsumption()
		{
			Func<ThingDef, int, string> lsName = delegate(ThingDef d, int lsIndex)
			{
				if (d.race.lifeStageAges.Count <= lsIndex)
				{
					return string.Empty;
				}
				LifeStageDef def = d.race.lifeStageAges[lsIndex].def;
				return def.defName;
			};
			Func<ThingDef, int, string> maxFood = delegate(ThingDef d, int lsIndex)
			{
				if (d.race.lifeStageAges.Count <= lsIndex)
				{
					return string.Empty;
				}
				LifeStageDef def = d.race.lifeStageAges[lsIndex].def;
				return (d.race.baseBodySize * def.bodySizeFactor * def.foodMaxFactor).ToString("F2");
			};
			Func<ThingDef, int, string> hungerRate = delegate(ThingDef d, int lsIndex)
			{
				if (d.race.lifeStageAges.Count <= lsIndex)
				{
					return string.Empty;
				}
				LifeStageDef def = d.race.lifeStageAges[lsIndex].def;
				return (d.race.baseHungerRate * def.hungerRateFactor).ToString("F2");
			};
			IEnumerable<ThingDef> arg_219_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.race != null && d.race.EatsFood
			orderby d.race.baseHungerRate descending
			select d;
			TableDataGetter<ThingDef>[] expr_BF = new TableDataGetter<ThingDef>[13];
			expr_BF[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_BF[1] = new TableDataGetter<ThingDef>("Lifestage 0", (ThingDef d) => lsName(d, 0));
			expr_BF[2] = new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 0));
			expr_BF[3] = new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 0));
			expr_BF[4] = new TableDataGetter<ThingDef>("Lifestage 1", (ThingDef d) => lsName(d, 1));
			expr_BF[5] = new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 1));
			expr_BF[6] = new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 1));
			expr_BF[7] = new TableDataGetter<ThingDef>("Lifestage 2", (ThingDef d) => lsName(d, 2));
			expr_BF[8] = new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 2));
			expr_BF[9] = new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 2));
			expr_BF[10] = new TableDataGetter<ThingDef>("Lifestage 3", (ThingDef d) => lsName(d, 3));
			expr_BF[11] = new TableDataGetter<ThingDef>("maxFood", (ThingDef d) => maxFood(d, 3));
			expr_BF[12] = new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => hungerRate(d, 3));
			DebugTables.MakeTablesDialog<ThingDef>(arg_219_0, expr_BF);
		}

		public static void DoTable_AnimalBiomeCommonalities()
		{
			List<TableDataGetter<PawnKindDef>> list = (from b in DefDatabase<BiomeDef>.AllDefs
			where b.implemented && b.canBuildBase
			orderby b.animalDensity
			select new TableDataGetter<PawnKindDef>(b.defName, delegate(PawnKindDef k)
			{
				float num = DefDatabase<PawnKindDef>.AllDefs.Sum((PawnKindDef ki) => b.CommonalityOfAnimal(ki));
				float num2 = b.CommonalityOfAnimal(k);
				float num3 = num2 / num;
				if (num3 == 0f)
				{
					return string.Empty;
				}
				return num3.ToStringPercent("F1");
			})).ToList<TableDataGetter<PawnKindDef>>();
			list.Insert(0, new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName + ((!k.race.race.predator) ? string.Empty : "*")));
			DebugTables.MakeTablesDialog<PawnKindDef>(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.defName
			select d, list.ToArray());
		}

		public static void DoTable_AnimalCombatBalance()
		{
			Func<PawnKindDef, float> meleeDps = delegate(PawnKindDef k)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(k, null);
				while (pawn.health.hediffSet.hediffs.Count > 0)
				{
					pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
				}
				return pawn.GetStatValue(StatDefOf.MeleeDPS, true);
			};
			Func<PawnKindDef, float> averageArmor = delegate(PawnKindDef k)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(k, null);
				while (pawn.health.hediffSet.hediffs.Count > 0)
				{
					pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs[0]);
				}
				return (pawn.GetStatValue(StatDefOf.ArmorRating_Blunt, true) + pawn.GetStatValue(StatDefOf.ArmorRating_Sharp, true)) / 2f;
			};
			Func<PawnKindDef, float> combatPowerCalculated = delegate(PawnKindDef k)
			{
				float num = 1f + meleeDps(k) * 2f;
				float num2 = 1f + (k.RaceProps.baseHealthScale + averageArmor(k) * 1.8f) * 2f;
				float num3 = num * num2 * 2.5f + 10f;
				return num3 + k.race.GetStatValueAbstract(StatDefOf.MoveSpeed, null) * 2f;
			};
			IEnumerable<PawnKindDef> arg_1A0_0 = from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.combatPower
			select d;
			TableDataGetter<PawnKindDef>[] expr_AD = new TableDataGetter<PawnKindDef>[7];
			expr_AD[0] = new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName);
			expr_AD[1] = new TableDataGetter<PawnKindDef>("meleeDps", (PawnKindDef k) => meleeDps(k).ToString("F1"));
			expr_AD[2] = new TableDataGetter<PawnKindDef>("baseHealthScale", (PawnKindDef k) => k.RaceProps.baseHealthScale.ToString());
			expr_AD[3] = new TableDataGetter<PawnKindDef>("moveSpeed", (PawnKindDef k) => k.race.GetStatValueAbstract(StatDefOf.MoveSpeed, null).ToString());
			expr_AD[4] = new TableDataGetter<PawnKindDef>("averageArmor", (PawnKindDef k) => averageArmor(k).ToStringPercent());
			expr_AD[5] = new TableDataGetter<PawnKindDef>("combatPowerCalculated", (PawnKindDef k) => combatPowerCalculated(k).ToString("F0"));
			expr_AD[6] = new TableDataGetter<PawnKindDef>("combatPower", (PawnKindDef k) => k.combatPower.ToString());
			DebugTables.MakeTablesDialog<PawnKindDef>(arg_1A0_0, expr_AD);
		}

		public static void DoTable_PlantsBasics()
		{
			IEnumerable<ThingDef> arg_19F_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Plant
			orderby d.plant.fertilitySensitivity
			select d;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[8];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("growDays", (ThingDef d) => d.plant.growDays.ToString("F2"));
			expr_4F[2] = new TableDataGetter<ThingDef>("reproduceMtb", (ThingDef d) => d.plant.reproduceMtbDays.ToString("F2"));
			expr_4F[3] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => (d.ingestible == null) ? "-" : d.ingestible.nutrition.ToString("F2"));
			expr_4F[4] = new TableDataGetter<ThingDef>("nut/day", (ThingDef d) => (d.ingestible == null) ? "-" : (d.ingestible.nutrition / d.plant.growDays).ToString("F4"));
			expr_4F[5] = new TableDataGetter<ThingDef>("fertilityMin", (ThingDef d) => d.plant.fertilityMin.ToString("F2"));
			expr_4F[6] = new TableDataGetter<ThingDef>("fertilitySensitivity", (ThingDef d) => d.plant.fertilitySensitivity.ToString("F2"));
			expr_4F[7] = new TableDataGetter<ThingDef>("blightable", (ThingDef d) => (!d.plant.Blightable) ? string.Empty : "blightable");
			DebugTables.MakeTablesDialog<ThingDef>(arg_19F_0, expr_4F);
		}

		public static void DoTable_WeaponPairs()
		{
			PawnWeaponGenerator.MakeTableWeaponPairs();
		}

		public static void DoTable_WeaponPairsByThing()
		{
			PawnWeaponGenerator.MakeTableWeaponPairsByThing();
		}

		public static void DoTable_WeaponsRanged()
		{
			Func<ThingDef, int> damage = (ThingDef d) => (d.Verbs[0].defaultProjectile == null) ? 0 : d.Verbs[0].defaultProjectile.projectile.damageAmountBase;
			Func<ThingDef, float> warmup = (ThingDef d) => d.Verbs[0].warmupTime;
			Func<ThingDef, float> cooldown = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, null);
			Func<ThingDef, int> burstShots = (ThingDef d) => d.Verbs[0].burstShotCount;
			Func<ThingDef, float> dpsMissless = delegate(ThingDef d)
			{
				int num = burstShots(d);
				float num2 = warmup(d) + cooldown(d);
				num2 += (float)(num - 1) * ((float)d.Verbs[0].ticksBetweenBurstShots / 60f);
				return (float)(damage(d) * num) / num2;
			};
			Func<ThingDef, float> accTouch = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyTouch, null);
			Func<ThingDef, float> accShort = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyShort, null);
			Func<ThingDef, float> accMed = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyMedium, null);
			Func<ThingDef, float> accLong = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyLong, null);
			Func<ThingDef, float> dpsAvg = delegate(ThingDef d)
			{
				float num = 0f;
				num += dpsMissless(d) * accShort(d);
				num += dpsMissless(d) * accMed(d);
				num += dpsMissless(d) * accMed(d);
				num += dpsMissless(d) * accLong(d);
				return num / 4f;
			};
			IEnumerable<ThingDef> arg_3A4_0 = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsRangedWeapon
			select d).OrderByDescending(dpsAvg);
			TableDataGetter<ThingDef>[] expr_17B = new TableDataGetter<ThingDef>[19];
			expr_17B[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_17B[1] = new TableDataGetter<ThingDef>("damage", (ThingDef d) => damage(d).ToString());
			expr_17B[2] = new TableDataGetter<ThingDef>("warmup", (ThingDef d) => warmup(d).ToString("F2"));
			expr_17B[3] = new TableDataGetter<ThingDef>("burst", (ThingDef d) => burstShots(d).ToString());
			expr_17B[4] = new TableDataGetter<ThingDef>("cooldown", (ThingDef d) => cooldown(d).ToString("F2"));
			expr_17B[5] = new TableDataGetter<ThingDef>("range", (ThingDef d) => d.Verbs[0].range.ToString("F0"));
			expr_17B[6] = new TableDataGetter<ThingDef>("dpsMissless", (ThingDef d) => dpsMissless(d).ToString("F2"));
			expr_17B[7] = new TableDataGetter<ThingDef>("accTouch", (ThingDef d) => accTouch(d).ToStringPercent());
			expr_17B[8] = new TableDataGetter<ThingDef>("accShort", (ThingDef d) => accShort(d).ToStringPercent());
			expr_17B[9] = new TableDataGetter<ThingDef>("accMed", (ThingDef d) => accMed(d).ToStringPercent());
			expr_17B[10] = new TableDataGetter<ThingDef>("accLong", (ThingDef d) => accLong(d).ToStringPercent());
			expr_17B[11] = new TableDataGetter<ThingDef>("dpsTouch", (ThingDef d) => (dpsMissless(d) * accTouch(d)).ToString("F2"));
			expr_17B[12] = new TableDataGetter<ThingDef>("dpsShort", (ThingDef d) => (dpsMissless(d) * accShort(d)).ToString("F2"));
			expr_17B[13] = new TableDataGetter<ThingDef>("dpsMed", (ThingDef d) => (dpsMissless(d) * accMed(d)).ToString("F2"));
			expr_17B[14] = new TableDataGetter<ThingDef>("dpsLong", (ThingDef d) => (dpsMissless(d) * accLong(d)).ToString("F2"));
			expr_17B[15] = new TableDataGetter<ThingDef>("dpsAvg", (ThingDef d) => dpsAvg(d).ToString("F2"));
			expr_17B[16] = new TableDataGetter<ThingDef>("mktVal", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, null).ToString("F0"));
			expr_17B[17] = new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, null).ToString("F0"));
			expr_17B[18] = new TableDataGetter<ThingDef>("mktVal/dpsAvg", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.MarketValue, null) / dpsAvg(d)).ToString("F2"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_3A4_0, expr_17B);
		}

		public static void DoTable_WeaponsMeleeStuffless()
		{
			DataAnalysisTableMaker.DoTablesInternal_Melee(null, false);
		}

		public static void DoTable_WeaponsMeleeSteel()
		{
			DataAnalysisTableMaker.DoTablesInternal_Melee(ThingDefOf.Steel, false);
		}

		public static void DoTable_WeaponsMeleeWood()
		{
			DataAnalysisTableMaker.DoTablesInternal_Melee(ThingDefOf.WoodLog, false);
		}

		public static void DoTable_WeaponsMeleePlasteel()
		{
			DataAnalysisTableMaker.DoTablesInternal_Melee(ThingDefOf.Plasteel, false);
		}

		public static void DoTable_MeleeSteelAndRaces()
		{
			DataAnalysisTableMaker.DoTablesInternal_Melee(ThingDefOf.Steel, true);
		}

		private static void DoTablesInternal_Melee(ThingDef stuff, bool doRaces = false)
		{
			Func<Def, float> meleeDamageGetter = delegate(Def d)
			{
				Thing owner;
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, out owner, stuff);
				if (concreteExampleVerbs.OfType<Verb_MeleeAttack>().Any<Verb_MeleeAttack>())
				{
					return concreteExampleVerbs.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeSelectionWeight(v, null, owner), (Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeDamageAmount(v, null, owner));
				}
				return -1f;
			};
			Func<Def, float> rangedDamageGetter = delegate(Def d)
			{
				Thing thing;
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, out thing, stuff);
				Verb verb = concreteExampleVerbs.OfType<Verb_LaunchProjectile>().FirstOrDefault<Verb_LaunchProjectile>();
				if (verb != null && verb.GetProjectile() != null)
				{
					return (float)verb.GetProjectile().projectile.damageAmountBase;
				}
				return -1f;
			};
			Func<Def, float> meleeWarmupGetter = (Def d) => 0f;
			Func<Def, float> rangedWarmupGetter = delegate(Def d)
			{
				Thing thing;
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, out thing, stuff);
				Verb verb = concreteExampleVerbs.OfType<Verb_LaunchProjectile>().FirstOrDefault<Verb_LaunchProjectile>();
				if (verb != null)
				{
					return verb.verbProps.warmupTime;
				}
				return -1f;
			};
			Func<Def, float> meleeCooldownGetter = delegate(Def d)
			{
				Thing owner;
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, out owner, stuff);
				if (concreteExampleVerbs.OfType<Verb_MeleeAttack>().Any<Verb_MeleeAttack>())
				{
					return concreteExampleVerbs.OfType<Verb_MeleeAttack>().AverageWeighted((Verb_MeleeAttack v) => v.verbProps.AdjustedMeleeSelectionWeight(v, null, owner), (Verb_MeleeAttack v) => v.verbProps.AdjustedCooldown(v, null, owner));
				}
				return -1f;
			};
			Func<Def, float> rangedCooldownGetter = delegate(Def d)
			{
				Thing thing;
				List<Verb> concreteExampleVerbs = VerbUtility.GetConcreteExampleVerbs(d, out thing, stuff);
				Verb verb = concreteExampleVerbs.OfType<Verb_LaunchProjectile>().FirstOrDefault<Verb_LaunchProjectile>();
				if (verb != null)
				{
					return verb.verbProps.defaultCooldownTime;
				}
				return -1f;
			};
			Func<Def, float> meleeDpsGetter = (Def d) => meleeDamageGetter(d) / (meleeWarmupGetter(d) + meleeCooldownGetter(d));
			Func<Def, float> rangedDpsGetter = (Def d) => rangedDamageGetter(d) / (rangedWarmupGetter(d) + rangedCooldownGetter(d));
			Func<Def, float> dpsGetter = (Def d) => Mathf.Max(meleeDpsGetter(d), rangedDpsGetter(d));
			Func<Def, float> marketValueGetter = delegate(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					return thingDef.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
				}
				HediffDef hediffDef = d as HediffDef;
				if (hediffDef == null)
				{
					return -1f;
				}
				if (hediffDef.spawnThingOnRemoved == null)
				{
					return 0f;
				}
				return hediffDef.spawnThingOnRemoved.GetStatValueAbstract(StatDefOf.MarketValue, null);
			};
			IEnumerable<Def> enumerable = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsMeleeWeapon
			select d).Cast<Def>().Concat((from h in DefDatabase<HediffDef>.AllDefs
			where h.CompProps<HediffCompProperties_VerbGiver>() != null
			select h).Cast<Def>());
			if (doRaces)
			{
				enumerable = enumerable.Concat((from d in DefDatabase<ThingDef>.AllDefs
				where d.race != null
				select d).Cast<Def>());
			}
			enumerable = from h in enumerable
			orderby dpsGetter(h) descending
			select h;
			IEnumerable<Def> arg_2C4_0 = enumerable;
			TableDataGetter<Def>[] expr_184 = new TableDataGetter<Def>[12];
			expr_184[0] = new TableDataGetter<Def>("defName", (Def d) => d.defName);
			expr_184[1] = new TableDataGetter<Def>("mDamage", (Def d) => meleeDamageGetter(d).ToString());
			expr_184[2] = new TableDataGetter<Def>("mWarmup", (Def d) => meleeWarmupGetter(d).ToString("F2"));
			expr_184[3] = new TableDataGetter<Def>("mCooldown", (Def d) => meleeCooldownGetter(d).ToString("F2"));
			expr_184[4] = new TableDataGetter<Def>("mDps", (Def d) => meleeDpsGetter(d).ToString("F2"));
			expr_184[5] = new TableDataGetter<Def>("rDamage", (Def d) => rangedDamageGetter(d).ToString());
			expr_184[6] = new TableDataGetter<Def>("rWarmup", (Def d) => rangedWarmupGetter(d).ToString("F2"));
			expr_184[7] = new TableDataGetter<Def>("rCooldown", (Def d) => rangedCooldownGetter(d).ToString("F2"));
			expr_184[8] = new TableDataGetter<Def>("rDps", (Def d) => rangedDpsGetter(d).ToString("F2"));
			expr_184[9] = new TableDataGetter<Def>("dps", (Def d) => dpsGetter(d).ToString("F2"));
			expr_184[10] = new TableDataGetter<Def>("mktval", (Def d) => marketValueGetter(d).ToString("F0"));
			expr_184[11] = new TableDataGetter<Def>("work", delegate(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef == null)
				{
					return "-";
				}
				return thingDef.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0");
			});
			DebugTables.MakeTablesDialog<Def>(arg_2C4_0, expr_184);
		}

		public static void DoTable_WeaponUsage()
		{
			List<TableDataGetter<PawnKindDef>> list = new List<TableDataGetter<PawnKindDef>>();
			list.Add(new TableDataGetter<PawnKindDef>("defName", (PawnKindDef x) => x.defName));
			list.AddRange(from x in DefDatabase<ThingDef>.AllDefs
			where x.IsWeapon && !x.weaponTags.NullOrEmpty<string>() && x.canBeSpawningInventory
			orderby x.IsMeleeWeapon descending, x.techLevel, x.BaseMarketValue
			select new TableDataGetter<PawnKindDef>(GenText.WithoutVowelsIfLong(x.label), delegate(PawnKindDef y)
			{
				if (!x.weaponTags.Any((string z) => y.weaponTags.Contains(z)))
				{
					return string.Empty;
				}
				if (y.weaponMoney.max < PawnWeaponGenerator.CheapestNonDerpPriceFor(x))
				{
					return "  no $";
				}
				return "   ✓";
			}));
			list.Add(new TableDataGetter<PawnKindDef>("avg $", (PawnKindDef x) => x.weaponMoney.Average.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("min $", (PawnKindDef x) => x.weaponMoney.min.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("max $", (PawnKindDef x) => x.weaponMoney.max.ToString()));
			list.Add(new TableDataGetter<PawnKindDef>("points", (PawnKindDef x) => x.combatPower.ToString()));
			DebugTables.MakeTablesDialog<PawnKindDef>(from x in DefDatabase<PawnKindDef>.AllDefs
			where !x.weaponTags.NullOrEmpty<string>()
			orderby (x.defaultFactionType == null) ? 2147483647 : ((int)x.defaultFactionType.techLevel), x.combatPower
			select x, list.ToArray());
		}

		public static void DoTable_ApparelPairs()
		{
			PawnApparelGenerator.MakeTableApparelPairs();
		}

		public static void DoTable_ApparelPairsByThing()
		{
			PawnApparelGenerator.MakeTableApparelPairsByThing();
		}

		public static void DoTable_ApparelPairsHeadwearLog()
		{
			PawnApparelGenerator.LogHeadwearApparelPairs();
		}

		public static void DoTable_ApparelSpawnStats()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef current in from pk in DefDatabase<PawnKindDef>.AllDefs
			where pk.race.race.Humanlike
			select pk)
			{
				PawnKindDef kind = current;
				list.Add(new FloatMenuOption(kind.defName, delegate
				{
					Faction faction = FactionUtility.DefaultFactionFrom(kind.defaultFactionType);
					DefMap<ThingDef, int> appCounts = new DefMap<ThingDef, int>();
					for (int i = 0; i < 200; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(kind, faction);
						foreach (Apparel current2 in pawn.apparel.WornApparel)
						{
							DefMap<ThingDef, int> appCounts2;
							ThingDef def;
							(appCounts2 = appCounts)[def = current2.def] = appCounts2[def] + 1;
						}
					}
					IEnumerable<ThingDef> arg_146_0 = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsApparel && appCounts[d] > 0).OrderByDescending((ThingDef d) => appCounts[d]);
					TableDataGetter<ThingDef>[] expr_DB = new TableDataGetter<ThingDef>[3];
					expr_DB[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
					expr_DB[1] = new TableDataGetter<ThingDef>("count of " + 200, (ThingDef d) => appCounts[d].ToString());
					expr_DB[2] = new TableDataGetter<ThingDef>("percent of pawns", (ThingDef d) => ((float)appCounts[d] / 200f).ToStringPercent("F2"));
					DebugTables.MakeTablesDialog<ThingDef>(arg_146_0, expr_DB);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoTable_ApparelStuffless()
		{
			DataAnalysisTableMaker.DoTablesInternal_Apparel(null);
		}

		public static void DoTable_ApparelCloth()
		{
			DataAnalysisTableMaker.DoTablesInternal_Apparel(ThingDefOf.Cloth);
		}

		public static void DoTable_ApparelHyperweave()
		{
			DataAnalysisTableMaker.DoTablesInternal_Apparel(ThingDefOf.Hyperweave);
		}

		public static void DoTable_ApparelHumanleather()
		{
			DataAnalysisTableMaker.DoTablesInternal_Apparel(ThingDef.Named("Human_Leather"));
		}

		private static void DoTablesInternal_Apparel(ThingDef stuff)
		{
			IEnumerable<ThingDef> arg_17A_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsApparel
			select d;
			TableDataGetter<ThingDef>[] expr_3B = new TableDataGetter<ThingDef>[10];
			expr_3B[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_3B[1] = new TableDataGetter<ThingDef>("bodyParts", (ThingDef d) => GenText.ToSpaceList(from bp in d.apparel.bodyPartGroups
			select bp.defName));
			expr_3B[2] = new TableDataGetter<ThingDef>("layers", (ThingDef d) => GenText.ToSpaceList(from l in d.apparel.layers
			select l.ToString()));
			expr_3B[3] = new TableDataGetter<ThingDef>("tags", (ThingDef d) => GenText.ToSpaceList(from t in d.apparel.tags
			select t.ToString()));
			expr_3B[4] = new TableDataGetter<ThingDef>("insCold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff).ToString("F0"));
			expr_3B[5] = new TableDataGetter<ThingDef>("insHeat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff).ToString("F0"));
			expr_3B[6] = new TableDataGetter<ThingDef>("blunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuff).ToString("F2"));
			expr_3B[7] = new TableDataGetter<ThingDef>("sharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuff).ToString("F2"));
			expr_3B[8] = new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, stuff).ToString("F0"));
			expr_3B[9] = new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_17A_0, expr_3B);
		}

		public static void DoTable_HitPoints()
		{
			IEnumerable<ThingDef> arg_CD_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.useHitPoints
			orderby d.GetStatValueAbstract(StatDefOf.MaxHitPoints, null) descending
			select d;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[3];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("hp", (ThingDef d) => d.BaseMaxHitPoints.ToString());
			expr_4F[2] = new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString());
			DebugTables.MakeTablesDialog<ThingDef>(arg_CD_0, expr_4F);
		}

		public static void DoTable_FillPercent()
		{
			IEnumerable<ThingDef> arg_CD_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.fillPercent > 0f
			orderby d.fillPercent descending
			select d;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[3];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("fillPercent", (ThingDef d) => d.fillPercent.ToStringPercent());
			expr_4F[2] = new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString());
			DebugTables.MakeTablesDialog<ThingDef>(arg_CD_0, expr_4F);
		}

		public static void DoTable_DeteriorationRates()
		{
			IEnumerable<ThingDef> arg_F7_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.GetStatValueAbstract(StatDefOf.DeteriorationRate, null) > 0f
			orderby d.GetStatValueAbstract(StatDefOf.DeteriorationRate, null) descending
			select d;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[4];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("deterioration rate", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.DeteriorationRate, null).ToString("F1"));
			expr_4F[2] = new TableDataGetter<ThingDef>("hp", (ThingDef d) => d.BaseMaxHitPoints.ToString());
			expr_4F[3] = new TableDataGetter<ThingDef>("days to vanish", (ThingDef d) => ((float)d.BaseMaxHitPoints / d.GetStatValueAbstract(StatDefOf.DeteriorationRate, null)).ToString("0.#"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_F7_0, expr_4F);
		}

		public static void DoTable_ShootingAccuracy()
		{
			StatDef stat = StatDefOf.ShootingAccuracy;
			Func<int, float, int, float> accAtDistance = delegate(int level, float dist, int traitDegree)
			{
				float num = 1f;
				if (traitDegree != 0)
				{
					float value = TraitDef.Named("ShootingAccuracy").DataAtDegree(traitDegree).statOffsets.First((StatModifier so) => so.stat == stat).value;
					num += value;
				}
				foreach (SkillNeed current in stat.skillNeedFactors)
				{
					SkillNeed_Direct skillNeed_Direct = current as SkillNeed_Direct;
					num *= skillNeed_Direct.valuesPerLevel[level];
				}
				num = stat.postProcessCurve.Evaluate(num);
				return Mathf.Pow(num, dist);
			};
			List<int> list = new List<int>();
			for (int i = 0; i <= 20; i++)
			{
				list.Add(i);
			}
			IEnumerable<int> arg_249_0 = list;
			TableDataGetter<int>[] expr_4B = new TableDataGetter<int>[18];
			expr_4B[0] = new TableDataGetter<int>("No trait skill", (int lev) => lev.ToString());
			expr_4B[1] = new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 0).ToStringPercent("F2"));
			expr_4B[2] = new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 0).ToStringPercent("F2"));
			expr_4B[3] = new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 0).ToStringPercent("F2"));
			expr_4B[4] = new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 0).ToStringPercent("F2"));
			expr_4B[5] = new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 0).ToStringPercent("F2"));
			expr_4B[6] = new TableDataGetter<int>("Careful shooter skill", (int lev) => lev.ToString());
			expr_4B[7] = new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 1).ToStringPercent("F2"));
			expr_4B[8] = new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 1).ToStringPercent("F2"));
			expr_4B[9] = new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 1).ToStringPercent("F2"));
			expr_4B[10] = new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 1).ToStringPercent("F2"));
			expr_4B[11] = new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 1).ToStringPercent("F2"));
			expr_4B[12] = new TableDataGetter<int>("Trigger-happy skill", (int lev) => lev.ToString());
			expr_4B[13] = new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, -1).ToStringPercent("F2"));
			expr_4B[14] = new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, -1).ToStringPercent("F2"));
			expr_4B[15] = new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, -1).ToStringPercent("F2"));
			expr_4B[16] = new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, -1).ToStringPercent("F2"));
			expr_4B[17] = new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, -1).ToStringPercent("F2"));
			DebugTables.MakeTablesDialog<int>(arg_249_0, expr_4B);
		}

		public static void DoTable_MiscIncidentChances()
		{
			List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				StorytellerComp_CategoryMTB storytellerComp_CategoryMTB = storytellerComps[i] as StorytellerComp_CategoryMTB;
				if (storytellerComp_CategoryMTB != null && ((StorytellerCompProperties_CategoryMTB)storytellerComp_CategoryMTB.props).category == IncidentCategory.Misc)
				{
					storytellerComp_CategoryMTB.DebugTablesIncidentChances(IncidentCategory.Misc);
				}
			}
		}

		public static void DoTable_BodyParts()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef current in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = current;
				list.Add(new FloatMenuOption(localBd.defName, delegate
				{
					IEnumerable<BodyPartRecord> arg_12F_0 = from d in localBd.AllParts
					orderby d.height descending
					select d;
					TableDataGetter<BodyPartRecord>[] expr_33 = new TableDataGetter<BodyPartRecord>[6];
					expr_33[0] = new TableDataGetter<BodyPartRecord>("defName", (BodyPartRecord d) => d.def.defName);
					expr_33[1] = new TableDataGetter<BodyPartRecord>("coverage", (BodyPartRecord d) => d.coverage.ToStringPercent());
					expr_33[2] = new TableDataGetter<BodyPartRecord>("coverageAbsWithChildren", (BodyPartRecord d) => d.coverageAbsWithChildren.ToStringPercent());
					expr_33[3] = new TableDataGetter<BodyPartRecord>("coverageAbs", (BodyPartRecord d) => d.coverageAbs.ToStringPercent());
					expr_33[4] = new TableDataGetter<BodyPartRecord>("depth", (BodyPartRecord d) => d.depth.ToString());
					expr_33[5] = new TableDataGetter<BodyPartRecord>("height", (BodyPartRecord d) => d.height.ToString());
					DebugTables.MakeTablesDialog<BodyPartRecord>(arg_12F_0, expr_33);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		private static void DoTable_FillPercents(ThingCategory cat)
		{
			IEnumerable<ThingDef> arg_7D_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == cat && !d.IsFrame && d.passability != Traversability.Impassable
			select d;
			TableDataGetter<ThingDef>[] expr_29 = new TableDataGetter<ThingDef>[2];
			expr_29[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_29[1] = new TableDataGetter<ThingDef>("fillPercent", (ThingDef d) => d.fillPercent.ToStringPercent());
			DebugTables.MakeTablesDialog<ThingDef>(arg_7D_0, expr_29);
		}

		public static void DoTable_ThingMasses()
		{
			IOrderedEnumerable<ThingDef> orderedEnumerable = from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.category == ThingCategory.Item || x.Minifiable
			where x.thingClass != typeof(MinifiedThing) && x.thingClass != typeof(UnfinishedThing)
			orderby x.GetStatValueAbstract(StatDefOf.Mass, null), x.GetStatValueAbstract(StatDefOf.MarketValue, null)
			select x;
			Func<ThingDef, float, string> perPawn = (ThingDef d, float bodySize) => (bodySize * 35f / d.GetStatValueAbstract(StatDefOf.Mass, null)).ToString("F0");
			Func<ThingDef, string> perNutrition = delegate(ThingDef d)
			{
				if (d.ingestible == null || d.ingestible.nutrition == 0f)
				{
					return string.Empty;
				}
				return (d.GetStatValueAbstract(StatDefOf.Mass, null) / d.ingestible.nutrition).ToString("F2");
			};
			IEnumerable<ThingDef> arg_1C3_0 = orderedEnumerable;
			TableDataGetter<ThingDef>[] expr_E1 = new TableDataGetter<ThingDef>[7];
			expr_E1[0] = new TableDataGetter<ThingDef>("defName", delegate(ThingDef d)
			{
				if (d.Minifiable)
				{
					return d.defName + " (minified)";
				}
				string text = d.defName;
				if (!d.EverHaulable)
				{
					text += " (not haulable)";
				}
				return text;
			});
			expr_E1[1] = new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Mass, null).ToString());
			expr_E1[2] = new TableDataGetter<ThingDef>("per human", (ThingDef d) => perPawn(d, ThingDefOf.Human.race.baseBodySize));
			expr_E1[3] = new TableDataGetter<ThingDef>("per muffalo", (ThingDef d) => perPawn(d, ThingDefOf.Muffalo.race.baseBodySize));
			expr_E1[4] = new TableDataGetter<ThingDef>("per dromedary", (ThingDef d) => perPawn(d, ThingDefOf.Dromedary.race.baseBodySize));
			expr_E1[5] = new TableDataGetter<ThingDef>("per nutrition", (ThingDef d) => perNutrition(d));
			expr_E1[6] = new TableDataGetter<ThingDef>("small volume", (ThingDef d) => (!d.smallVolume) ? string.Empty : "small");
			DebugTables.MakeTablesDialog<ThingDef>(arg_1C3_0, expr_E1);
		}

		public static void DoTable_MedicalPotencyPerMedicine()
		{
			List<float> list = new List<float>();
			list.Add(0.3f);
			list.AddRange(from d in DefDatabase<ThingDef>.AllDefs
			where typeof(Medicine).IsAssignableFrom(d.thingClass)
			select d.GetStatValueAbstract(StatDefOf.MedicalPotency, null));
			SkillNeed_Direct skillNeed_Direct = (SkillNeed_Direct)StatDefOf.MedicalTendQuality.skillNeedFactors[0];
			TableDataGetter<float>[] array = new TableDataGetter<float>[21];
			array[0] = new TableDataGetter<float>("potency", new Func<float, string>(GenText.ToStringPercent));
			for (int i = 0; i < 20; i++)
			{
				float factor = skillNeed_Direct.valuesPerLevel[i];
				array[i + 1] = new TableDataGetter<float>((i + 1).ToString(), (float p) => (p * factor).ToStringPercent());
			}
			DebugTables.MakeTablesDialog<float>(list, array);
		}

		public static void DoTable_BuildingFillpercents()
		{
			DataAnalysisTableMaker.DoTable_FillPercents(ThingCategory.Building);
		}

		public static void DoTable_ItemFillpercents()
		{
			DataAnalysisTableMaker.DoTable_FillPercents(ThingCategory.Item);
		}

		public static void DoTable_TraderKinds()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			foreach (TraderKindDef current in DefDatabase<TraderKindDef>.AllDefs)
			{
				TraderKindDef localTk = current;
				string text = localTk.defName;
				text = text.Replace("Caravan", "C");
				text = text.Replace("Visitor", "V");
				text = text.Replace("Orbital", "R");
				text = text.Replace("Neolithic", "N");
				text = text.Replace("Outlander", "O");
				text = GenText.WithoutVowels(text);
				list.Add(new TableDataGetter<ThingDef>(text, (ThingDef td) => (!localTk.WillTrade(td)) ? string.Empty : "✓"));
			}
			DebugTables.MakeTablesDialog<ThingDef>(from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.001f && !d.isUnfinishedThing && !d.IsCorpse && !d.destroyOnDrop && d != ThingDefOf.Silver && !d.thingCategories.NullOrEmpty<ThingCategoryDef>()) || (d.category == ThingCategory.Building && d.Minifiable)
			orderby d.thingCategories.NullOrEmpty<ThingCategoryDef>() ? "zzzzzzz" : d.thingCategories[0].defName, d.BaseMarketValue
			select d, list.ToArray());
		}

		public static void DoTable_Surgeries()
		{
			Func<RecipeDef, float> trueWork = (RecipeDef d) => d.WorkAmountTotal(null);
			IEnumerable<RecipeDef> arg_14C_0 = (from d in DefDatabase<RecipeDef>.AllDefs
			where d.IsSurgery
			select d).OrderByDescending(trueWork);
			TableDataGetter<RecipeDef>[] expr_61 = new TableDataGetter<RecipeDef>[6];
			expr_61[0] = new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName);
			expr_61[1] = new TableDataGetter<RecipeDef>("work", (RecipeDef d) => trueWork(d).ToString("F0"));
			expr_61[2] = new TableDataGetter<RecipeDef>("ingredients", (RecipeDef d) => GenText.ToCommaList(from ing in d.ingredients
			select ing.ToString(), false));
			expr_61[3] = new TableDataGetter<RecipeDef>("skillRequirements", delegate(RecipeDef d)
			{
				string arg_43_0;
				if (d.skillRequirements == null)
				{
					arg_43_0 = "-";
				}
				else
				{
					arg_43_0 = GenText.ToCommaList(from ing in d.skillRequirements
					select ing.ToString(), false);
				}
				return arg_43_0;
			});
			expr_61[4] = new TableDataGetter<RecipeDef>("surgerySuccessChanceFactor", (RecipeDef d) => d.surgerySuccessChanceFactor.ToStringPercent());
			expr_61[5] = new TableDataGetter<RecipeDef>("deathOnFailChance", (RecipeDef d) => d.deathOnFailedSurgeryChance.ToStringPercent());
			DebugTables.MakeTablesDialog<RecipeDef>(arg_14C_0, expr_61);
		}

		public static void DoTable_Terrains()
		{
			IEnumerable<TerrainDef> arg_B3_0 = DefDatabase<TerrainDef>.AllDefs;
			TableDataGetter<TerrainDef>[] expr_0B = new TableDataGetter<TerrainDef>[4];
			expr_0B[0] = new TableDataGetter<TerrainDef>("defName", (TerrainDef d) => d.defName);
			expr_0B[1] = new TableDataGetter<TerrainDef>("beauty", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Beauty, null).ToString());
			expr_0B[2] = new TableDataGetter<TerrainDef>("cleanliness", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Cleanliness, null).ToString());
			expr_0B[3] = new TableDataGetter<TerrainDef>("pathCost", (TerrainDef d) => d.pathCost.ToString());
			DebugTables.MakeTablesDialog<TerrainDef>(arg_B3_0, expr_0B);
		}

		public static void DoTable_MentalBreaksMinor()
		{
			DataAnalysisTableMaker.DoMentalBreaksTable(from x in DefDatabase<MentalBreakDef>.AllDefs
			where x.intensity == MentalBreakIntensity.Minor
			select x);
		}

		public static void DoTable_MentalBreaksMajor()
		{
			DataAnalysisTableMaker.DoMentalBreaksTable(from x in DefDatabase<MentalBreakDef>.AllDefs
			where x.intensity == MentalBreakIntensity.Major
			select x);
		}

		public static void DoTable_MentalBreaksExtreme()
		{
			DataAnalysisTableMaker.DoMentalBreaksTable(from x in DefDatabase<MentalBreakDef>.AllDefs
			where x.intensity == MentalBreakIntensity.Extreme
			select x);
		}

		public static void DoTable_BestThingRequestGroup()
		{
			IEnumerable<ThingDef> arg_CD_0 = from x in DefDatabase<ThingDef>.AllDefs
			where ListerThings.EverListable(x, ListerThingsUse.Global) || ListerThings.EverListable(x, ListerThingsUse.Region)
			orderby x.label
			select x;
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[3];
			expr_4F[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("best local", delegate(ThingDef d)
			{
				IEnumerable<ThingRequestGroup> source;
				if (!ListerThings.EverListable(d, ListerThingsUse.Region))
				{
					source = Enumerable.Empty<ThingRequestGroup>();
				}
				else
				{
					source = from x in (ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))
					where x.StoreInRegion() && x.Includes(d)
					select x;
				}
				if (!source.Any<ThingRequestGroup>())
				{
					return "-";
				}
				ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Region) && x.Includes(y)));
				return string.Concat(new object[]
				{
					best,
					" (defs: ",
					DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Region) && best.Includes(x)),
					")"
				});
			});
			expr_4F[2] = new TableDataGetter<ThingDef>("best global", delegate(ThingDef d)
			{
				IEnumerable<ThingRequestGroup> source;
				if (!ListerThings.EverListable(d, ListerThingsUse.Global))
				{
					source = Enumerable.Empty<ThingRequestGroup>();
				}
				else
				{
					source = from x in (ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup))
					where x.Includes(d)
					select x;
				}
				if (!source.Any<ThingRequestGroup>())
				{
					return "-";
				}
				ThingRequestGroup best = source.MinBy((ThingRequestGroup x) => DefDatabase<ThingDef>.AllDefs.Count((ThingDef y) => ListerThings.EverListable(y, ListerThingsUse.Global) && x.Includes(y)));
				return string.Concat(new object[]
				{
					best,
					" (defs: ",
					DefDatabase<ThingDef>.AllDefs.Count((ThingDef x) => ListerThings.EverListable(x, ListerThingsUse.Global) && best.Includes(x)),
					")"
				});
			});
			DebugTables.MakeTablesDialog<ThingDef>(arg_CD_0, expr_4F);
		}

		public static void DoTable_Drugs()
		{
			IEnumerable<ThingDef> arg_AB_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsDrug
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[3];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("pleasue", (ThingDef d) => (!d.IsPleasureDrug) ? string.Empty : "pleasure");
			expr_2D[2] = new TableDataGetter<ThingDef>("non-medical", (ThingDef d) => (!d.IsNonMedicalDrug) ? string.Empty : "non-medical");
			DebugTables.MakeTablesDialog<ThingDef>(arg_AB_0, expr_2D);
		}

		public static void DoTable_PawnGroupsMadeRepeated()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				if (current.def.pawnGroupMakers != null)
				{
					if (current.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Normal))
					{
						Faction localFac = current;
						list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
						{
							List<DebugMenuOption> list2 = new List<DebugMenuOption>();
							foreach (float localP2 in Dialog_DebugActionsMenu.PointsOptions())
							{
								float localP = localP2;
								list2.Add(new DebugMenuOption(localP.ToString(), DebugMenuOptionMode.Action, delegate
								{
									Dictionary<ThingDef, int>[] weaponsCount = new Dictionary<ThingDef, int>[20];
									string[] pawnKinds = new string[20];
									for (int i = 0; i < 20; i++)
									{
										weaponsCount[i] = new Dictionary<ThingDef, int>();
										PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
										pawnGroupMakerParms.tile = Find.VisibleMap.Tile;
										pawnGroupMakerParms.points = localP;
										pawnGroupMakerParms.faction = localFac;
										List<Pawn> list3 = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, pawnGroupMakerParms, false).ToList<Pawn>();
										pawnKinds[i] = PawnUtility.PawnKindsToCommaList(list3);
										foreach (Pawn current2 in list3)
										{
											if (current2.equipment.Primary != null)
											{
												if (!weaponsCount[i].ContainsKey(current2.equipment.Primary.def))
												{
													weaponsCount[i].Add(current2.equipment.Primary.def, 0);
												}
												Dictionary<ThingDef, int> dictionary;
												ThingDef def;
												(dictionary = weaponsCount[i])[def = current2.equipment.Primary.def] = dictionary[def] + 1;
											}
											current2.Destroy(DestroyMode.Vanish);
										}
									}
									int totalPawns = weaponsCount.Sum((Dictionary<ThingDef, int> x) => x.Sum((KeyValuePair<ThingDef, int> y) => y.Value));
									List<TableDataGetter<int>> list4 = new List<TableDataGetter<int>>();
									list4.Add(new TableDataGetter<int>(string.Empty, (int x) => (x != 20) ? (x + 1).ToString() : "avg"));
									list4.Add(new TableDataGetter<int>("pawns", delegate(int x)
									{
										string arg_64_0 = " ";
										string arg_64_1;
										if (x == 20)
										{
											arg_64_1 = ((float)totalPawns / 20f).ToString("0.#");
										}
										else
										{
											arg_64_1 = weaponsCount[x].Sum((KeyValuePair<ThingDef, int> y) => y.Value).ToString();
										}
										return arg_64_0 + arg_64_1;
									}));
									list4.AddRange(from x in DefDatabase<ThingDef>.AllDefs
									where x.IsWeapon && !x.weaponTags.NullOrEmpty<string>() && x.canBeSpawningInventory
									orderby x.IsMeleeWeapon descending, x.techLevel, x.BaseMarketValue
									select new TableDataGetter<int>(GenText.WithoutVowelsIfLong(x.label), delegate(int y)
									{
										if (y == 20)
										{
											return " " + ((float)weaponsCount.Sum((Dictionary<ThingDef, int> z) => (!z.ContainsKey(x)) ? 0 : z[x]) / 20f).ToString("0.#");
										}
										string arg_104_0;
										if (weaponsCount[y].ContainsKey(x))
										{
											object[] expr_66 = new object[5];
											expr_66[0] = " ";
											expr_66[1] = weaponsCount[y][x];
											expr_66[2] = " (";
											expr_66[3] = ((float)weaponsCount[y][x] / (float)weaponsCount[y].Sum((KeyValuePair<ThingDef, int> z) => z.Value)).ToStringPercent("F0");
											expr_66[4] = ")";
											arg_104_0 = string.Concat(expr_66);
										}
										else
										{
											arg_104_0 = string.Empty;
										}
										return arg_104_0;
									}));
									list4.Add(new TableDataGetter<int>("kinds", (int x) => (x != 20) ? pawnKinds[x] : string.Empty));
									DebugTables.MakeTablesDialog<int>(Enumerable.Range(0, 21), list4.ToArray());
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
						}));
					}
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		public static void DoTable_ItemAccessibility()
		{
			IEnumerable<ThingDef> arg_129_0 = from x in ItemCollectionGeneratorUtility.allGeneratableItems
			orderby x.defName
			select x;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[6];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("1", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 1, Find.VisibleMap)) ? string.Empty : "✓");
			expr_2D[2] = new TableDataGetter<ThingDef>("10", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 10, Find.VisibleMap)) ? string.Empty : "✓");
			expr_2D[3] = new TableDataGetter<ThingDef>("100", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 100, Find.VisibleMap)) ? string.Empty : "✓");
			expr_2D[4] = new TableDataGetter<ThingDef>("1000", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 1000, Find.VisibleMap)) ? string.Empty : "✓");
			expr_2D[5] = new TableDataGetter<ThingDef>("10000", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 10000, Find.VisibleMap)) ? string.Empty : "✓");
			DebugTables.MakeTablesDialog<ThingDef>(arg_129_0, expr_2D);
		}

		private static string ToStringEmptyZero(this float f, string format)
		{
			if (f <= 0f)
			{
				return string.Empty;
			}
			return f.ToString(format);
		}

		private static string ToStringPercentEmptyZero(this float f, string format = "F0")
		{
			if (f <= 0f)
			{
				return string.Empty;
			}
			return f.ToStringPercent(format);
		}

		private static void DoMentalBreaksTable(IEnumerable<MentalBreakDef> breaks)
		{
			float sumWeights = breaks.Sum((MentalBreakDef x) => x.baseCommonality);
			TableDataGetter<MentalBreakDef>[] expr_37 = new TableDataGetter<MentalBreakDef>[10];
			expr_37[0] = new TableDataGetter<MentalBreakDef>("defName", (MentalBreakDef d) => d.defName);
			expr_37[1] = new TableDataGetter<MentalBreakDef>("chance", (MentalBreakDef d) => (d.baseCommonality / sumWeights).ToStringPercent());
			expr_37[2] = new TableDataGetter<MentalBreakDef>("min duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.minTicksBeforeRecovery / 60000f).ToString("0.##") : string.Empty);
			expr_37[3] = new TableDataGetter<MentalBreakDef>("avg duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? (Mathf.Min((float)d.mentalState.minTicksBeforeRecovery + d.mentalState.recoveryMtbDays * 60000f, (float)d.mentalState.maxTicksBeforeRecovery) / 60000f).ToString("0.##") : string.Empty);
			expr_37[4] = new TableDataGetter<MentalBreakDef>("max duration (days)", (MentalBreakDef d) => (d.mentalState != null) ? ((float)d.mentalState.maxTicksBeforeRecovery / 60000f).ToString("0.##") : string.Empty);
			expr_37[5] = new TableDataGetter<MentalBreakDef>("recoverFromSleep", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.recoverFromSleep) ? string.Empty : "recoverFromSleep");
			expr_37[6] = new TableDataGetter<MentalBreakDef>("recoveryThought", (MentalBreakDef d) => (d.mentalState != null) ? d.mentalState.moodRecoveryThought.ToStringSafe<ThoughtDef>() : string.Empty);
			expr_37[7] = new TableDataGetter<MentalBreakDef>("aggro", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.IsAggro) ? string.Empty : "aggro");
			expr_37[8] = new TableDataGetter<MentalBreakDef>("blockNormalThoughts", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.blockNormalThoughts) ? string.Empty : "blockNormalThoughts");
			expr_37[9] = new TableDataGetter<MentalBreakDef>("allowBeatfire", (MentalBreakDef d) => (d.mentalState == null || !d.mentalState.allowBeatfire) ? string.Empty : "allowBeatfire");
			DebugTables.MakeTablesDialog<MentalBreakDef>(breaks, expr_37);
		}
	}
}
