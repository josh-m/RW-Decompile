using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsEconomy
	{
		[Category("Economy"), DebugOutput]
		public static void RecipeSkills()
		{
			IEnumerable<RecipeDef> arg_DD_0 = DefDatabase<RecipeDef>.AllDefs;
			TableDataGetter<RecipeDef>[] expr_0B = new TableDataGetter<RecipeDef>[5];
			expr_0B[0] = new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName);
			expr_0B[1] = new TableDataGetter<RecipeDef>("workSkill", (RecipeDef d) => (d.workSkill != null) ? d.workSkill.defName : string.Empty);
			expr_0B[2] = new TableDataGetter<RecipeDef>("workSpeedStat", (RecipeDef d) => (d.workSpeedStat != null) ? d.workSpeedStat.defName : string.Empty);
			expr_0B[3] = new TableDataGetter<RecipeDef>("workSpeedStat's skillNeedFactors", delegate(RecipeDef d)
			{
				string arg_67_0;
				if (d.workSpeedStat == null)
				{
					arg_67_0 = string.Empty;
				}
				else if (d.workSpeedStat.skillNeedFactors.NullOrEmpty<SkillNeed>())
				{
					arg_67_0 = string.Empty;
				}
				else
				{
					arg_67_0 = (from fac in d.workSpeedStat.skillNeedFactors
					select fac.skill.defName).ToCommaList(false);
				}
				return arg_67_0;
			});
			expr_0B[4] = new TableDataGetter<RecipeDef>("workSkillLearnFactor", (RecipeDef d) => d.workSkillLearnFactor);
			DebugTables.MakeTablesDialog<RecipeDef>(arg_DD_0, expr_0B);
		}

		[Category("Economy"), DebugOutput]
		public static void Drugs()
		{
			Func<ThingDef, float> realIngredientCost = (ThingDef d) => DebugOutputsEconomy.CostToMake(d, true);
			Func<ThingDef, float> realSellPrice = (ThingDef d) => d.BaseMarketValue * 0.6f;
			Func<ThingDef, float> realBuyPrice = (ThingDef d) => d.BaseMarketValue * 1.4f;
			IEnumerable<ThingDef> arg_197_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsWithinCategory(ThingCategoryDefOf.Medicine) || d.IsWithinCategory(ThingCategoryDefOf.Drugs)
			select d;
			TableDataGetter<ThingDef>[] expr_9C = new TableDataGetter<ThingDef>[8];
			expr_9C[0] = new TableDataGetter<ThingDef>("name", (ThingDef d) => d.defName);
			expr_9C[1] = new TableDataGetter<ThingDef>("ingredients", (ThingDef d) => DebugOutputsEconomy.CostListString(d, true, true));
			expr_9C[2] = new TableDataGetter<ThingDef>("work amount", (ThingDef d) => DebugOutputsEconomy.WorkToProduceBest(d).ToString("F0"));
			expr_9C[3] = new TableDataGetter<ThingDef>("real ingredient cost", (ThingDef d) => realIngredientCost(d).ToString("F1"));
			expr_9C[4] = new TableDataGetter<ThingDef>("real sell price", (ThingDef d) => realSellPrice(d).ToString("F1"));
			expr_9C[5] = new TableDataGetter<ThingDef>("real profit per item", (ThingDef d) => (realSellPrice(d) - realIngredientCost(d)).ToString("F1"));
			expr_9C[6] = new TableDataGetter<ThingDef>("real profit per day's work", (ThingDef d) => ((realSellPrice(d) - realIngredientCost(d)) / DebugOutputsEconomy.WorkToProduceBest(d) * 30000f).ToString("F1"));
			expr_9C[7] = new TableDataGetter<ThingDef>("real buy price", (ThingDef d) => realBuyPrice(d).ToString("F1"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_197_0, expr_9C);
		}

		[Category("Economy"), DebugOutput]
		public static void Wool()
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

		[Category("Economy"), DebugOutput]
		public static void AnimalGrowth()
		{
			Func<ThingDef, float> gestDaysEach = new Func<ThingDef, float>(DebugOutputsEconomy.GestationDaysEach);
			Func<ThingDef, float> nutritionToGestate = delegate(ThingDef d)
			{
				float num = 0f;
				LifeStageAge lifeStageAge = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1];
				return num + gestDaysEach(d) * lifeStageAge.def.hungerRateFactor * d.race.baseHungerRate;
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
				return compProperties.eggFertilizedDef.GetStatValueAbstract(StatDefOf.Nutrition, null).ToString("F2");
			};
			IEnumerable<ThingDef> arg_352_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.IsFlesh
			orderby bestMeatPerInput(d) descending
			select d;
			TableDataGetter<ThingDef>[] expr_108 = new TableDataGetter<ThingDef>[17];
			expr_108[0] = new TableDataGetter<ThingDef>(string.Empty, (ThingDef d) => d.defName);
			expr_108[1] = new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => d.race.baseHungerRate.ToString("F2"));
			expr_108[2] = new TableDataGetter<ThingDef>("gestDaysEach", (ThingDef d) => gestDaysEach(d).ToString("F2"));
			expr_108[3] = new TableDataGetter<ThingDef>("herbiv", (ThingDef d) => ((d.race.foodType & FoodTypeFlags.Plant) == FoodTypeFlags.None) ? string.Empty : "Y");
			expr_108[4] = new TableDataGetter<ThingDef>("|", (ThingDef d) => "|");
			expr_108[5] = new TableDataGetter<ThingDef>("bodySize", (ThingDef d) => d.race.baseBodySize.ToString("F2"));
			expr_108[6] = new TableDataGetter<ThingDef>("age Adult", (ThingDef d) => d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge.ToString("F2"));
			expr_108[7] = new TableDataGetter<ThingDef>("nutrition to adulthood", (ThingDef d) => nutritionToAdulthood(d).ToString("F2"));
			expr_108[8] = new TableDataGetter<ThingDef>("adult meat-nut", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.MeatAmount, null) * 0.05f).ToString("F2"));
			expr_108[9] = new TableDataGetter<ThingDef>("adult meat-nut / input-nut", (ThingDef d) => adultMeatNutPerInput(d).ToString("F3"));
			expr_108[10] = new TableDataGetter<ThingDef>("|", (ThingDef d) => "|");
			expr_108[11] = new TableDataGetter<ThingDef>("baby size", (ThingDef d) => (d.race.lifeStageAges[0].def.bodySizeFactor * d.race.baseBodySize).ToString("F2"));
			expr_108[12] = new TableDataGetter<ThingDef>("nutrition to gestate", (ThingDef d) => nutritionToGestate(d).ToString("F2"));
			expr_108[13] = new TableDataGetter<ThingDef>("egg nut", (ThingDef d) => eggNut(d));
			expr_108[14] = new TableDataGetter<ThingDef>("baby meat-nut", (ThingDef d) => babyMeatNut(d).ToString("F2"));
			expr_108[15] = new TableDataGetter<ThingDef>("baby meat-nut / input-nut", (ThingDef d) => babyMeatNutPerInput(d).ToString("F2"));
			expr_108[16] = new TableDataGetter<ThingDef>("baby wins", (ThingDef d) => (babyMeatNutPerInput(d) <= adultMeatNutPerInput(d)) ? string.Empty : "B");
			DebugTables.MakeTablesDialog<ThingDef>(arg_352_0, expr_108);
		}

		[Category("Economy"), DebugOutput]
		public static void AnimalBreeding()
		{
			IEnumerable<ThingDef> arg_14B_0 = (from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.IsFlesh
			select d).OrderByDescending(new Func<ThingDef, float>(DebugOutputsEconomy.GestationDaysEach));
			TableDataGetter<ThingDef>[] expr_4F = new TableDataGetter<ThingDef>[6];
			expr_4F[0] = new TableDataGetter<ThingDef>(string.Empty, (ThingDef d) => d.defName);
			expr_4F[1] = new TableDataGetter<ThingDef>("gestDaysEach", (ThingDef d) => DebugOutputsEconomy.GestationDaysEach(d).ToString("F2"));
			expr_4F[2] = new TableDataGetter<ThingDef>("avgOffspring", (ThingDef d) => (!d.HasComp(typeof(CompEggLayer))) ? ((d.race.litterSizeCurve == null) ? 1f : Rand.ByCurveAverage(d.race.litterSizeCurve)).ToString("F2") : d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average.ToString("F2"));
			expr_4F[3] = new TableDataGetter<ThingDef>("gestDaysRaw", (ThingDef d) => (!d.HasComp(typeof(CompEggLayer))) ? d.race.gestationPeriodDays.ToString("F1") : d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays.ToString("F1"));
			expr_4F[4] = new TableDataGetter<ThingDef>("growth per 30d", delegate(ThingDef d)
			{
				float f = 1f + ((!d.HasComp(typeof(CompEggLayer))) ? ((d.race.litterSizeCurve == null) ? 1f : Rand.ByCurveAverage(d.race.litterSizeCurve)) : d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average);
				float num = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f;
				float num2 = num + ((!d.HasComp(typeof(CompEggLayer))) ? d.race.gestationPeriodDays : d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays);
				float p = 30f / num2;
				return Mathf.Pow(f, p).ToString("F2");
			});
			expr_4F[5] = new TableDataGetter<ThingDef>("growth per 60d", delegate(ThingDef d)
			{
				float f = 1f + ((!d.HasComp(typeof(CompEggLayer))) ? ((d.race.litterSizeCurve == null) ? 1f : Rand.ByCurveAverage(d.race.litterSizeCurve)) : d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average);
				float num = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f;
				float num2 = num + ((!d.HasComp(typeof(CompEggLayer))) ? d.race.gestationPeriodDays : d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays);
				float p = 60f / num2;
				return Mathf.Pow(f, p).ToString("F2");
			});
			DebugTables.MakeTablesDialog<ThingDef>(arg_14B_0, expr_4F);
		}

		private static float GestationDaysEach(ThingDef d)
		{
			if (d.HasComp(typeof(CompEggLayer)))
			{
				CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
				return compProperties.eggLayIntervalDays / compProperties.eggCountRange.Average;
			}
			return d.race.gestationPeriodDays / ((d.race.litterSizeCurve == null) ? 1f : Rand.ByCurveAverage(d.race.litterSizeCurve));
		}

		[Category("Economy"), DebugOutput]
		public static void BuildingSkills()
		{
			IEnumerable<BuildableDef> arg_95_0 = from d in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
			where d.BuildableByPlayer
			select d;
			TableDataGetter<BuildableDef>[] expr_41 = new TableDataGetter<BuildableDef>[2];
			expr_41[0] = new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName);
			expr_41[1] = new TableDataGetter<BuildableDef>("construction skill prerequisite", (BuildableDef d) => d.constructionSkillPrerequisite);
			DebugTables.MakeTablesDialog<BuildableDef>(arg_95_0, expr_41);
		}

		[Category("Economy"), DebugOutput]
		public static void Crops()
		{
			Func<ThingDef, float> workCost = delegate(ThingDef d)
			{
				float num = 1.1f;
				num += d.plant.growDays * 1f;
				return num + (d.plant.sowWork + d.plant.harvestWork) * 0.00612f;
			};
			IEnumerable<ThingDef> arg_297_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Plant && d.plant.Harvestable && d.plant.Sowable
			orderby d.plant.IsTree
			select d;
			TableDataGetter<ThingDef>[] expr_79 = new TableDataGetter<ThingDef>[14];
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
			expr_79[10] = new TableDataGetter<ThingDef>("nutrition per growDay", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? string.Empty : (d.plant.harvestYield * d.plant.harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition, null) / d.plant.growDays).ToString("F2"));
			expr_79[11] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? string.Empty : d.plant.harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition, null).ToString("F2"));
			expr_79[12] = new TableDataGetter<ThingDef>("fertMin", (ThingDef d) => d.plant.fertilityMin.ToStringPercent());
			expr_79[13] = new TableDataGetter<ThingDef>("fertSensitivity", (ThingDef d) => d.plant.fertilitySensitivity.ToStringPercent());
			DebugTables.MakeTablesDialog<ThingDef>(arg_297_0, expr_79);
		}

		[Category("Economy"), DebugOutput]
		public static void ItemAndBuildingAcquisition()
		{
			Func<ThingDef, string> recipes = delegate(ThingDef d)
			{
				List<string> list = new List<string>();
				foreach (RecipeDef current in DefDatabase<RecipeDef>.AllDefs)
				{
					if (!current.products.NullOrEmpty<ThingDefCountClass>())
					{
						for (int i = 0; i < current.products.Count; i++)
						{
							if (current.products[i].thingDef == d)
							{
								list.Add(current.defName);
							}
						}
					}
				}
				if (list.Count == 0)
				{
					return string.Empty;
				}
				return list.ToCommaList(false);
			};
			Func<ThingDef, string> workAmountSources = delegate(ThingDef d)
			{
				List<string> list = new List<string>();
				if (d.StatBaseDefined(StatDefOf.WorkToMake))
				{
					list.Add("WorkToMake(" + d.GetStatValueAbstract(StatDefOf.WorkToMake, null) + ")");
				}
				if (d.StatBaseDefined(StatDefOf.WorkToBuild))
				{
					list.Add("WorkToBuild(" + d.GetStatValueAbstract(StatDefOf.WorkToBuild, null) + ")");
				}
				foreach (RecipeDef current in DefDatabase<RecipeDef>.AllDefs)
				{
					if (current.workAmount > 0f && !current.products.NullOrEmpty<ThingDefCountClass>())
					{
						for (int i = 0; i < current.products.Count; i++)
						{
							if (current.products[i].thingDef == d)
							{
								list.Add(string.Concat(new object[]
								{
									"RecipeDef-",
									current.defName,
									"(",
									current.workAmount,
									")"
								}));
							}
						}
					}
				}
				if (list.Count == 0)
				{
					return string.Empty;
				}
				return list.ToCommaList(false);
			};
			Func<ThingDef, string> calculatedMarketValue = delegate(ThingDef d)
			{
				if (!DebugOutputsEconomy.Producible(d))
				{
					return "not producible";
				}
				if (!d.StatBaseDefined(StatDefOf.MarketValue))
				{
					return "used";
				}
				string text = StatWorker_MarketValue.CalculatedBaseMarketValue(d, null).ToString("F1");
				if (StatWorker_MarketValue.CalculableRecipe(d) != null)
				{
					return text + " (recipe)";
				}
				return text;
			};
			IEnumerable<ThingDef> arg_333_0 = from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.01f) || (d.category == ThingCategory.Building && (d.BuildableByPlayer || d.Minifiable))
			orderby d.BaseMarketValue
			select d;
			TableDataGetter<ThingDef>[] expr_BF = new TableDataGetter<ThingDef>[16];
			expr_BF[0] = new TableDataGetter<ThingDef>("cat.", (ThingDef d) => d.category.ToString().Substring(0, 1).CapitalizeFirst());
			expr_BF[1] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_BF[2] = new TableDataGetter<ThingDef>("mobile", (ThingDef d) => (d.category == ThingCategory.Item || d.Minifiable).ToStringCheckBlank());
			expr_BF[3] = new TableDataGetter<ThingDef>("base\nmarket value", (ThingDef d) => d.BaseMarketValue.ToString("F1"));
			expr_BF[4] = new TableDataGetter<ThingDef>("calculated\nmarket value", (ThingDef d) => calculatedMarketValue(d));
			expr_BF[5] = new TableDataGetter<ThingDef>("cost to make", (ThingDef d) => DebugOutputsEconomy.CostToMakeString(d, false));
			expr_BF[6] = new TableDataGetter<ThingDef>("work to produce", (ThingDef d) => (DebugOutputsEconomy.WorkToProduceBest(d) <= 0f) ? "-" : DebugOutputsEconomy.WorkToProduceBest(d).ToString("F1"));
			expr_BF[7] = new TableDataGetter<ThingDef>("profit", (ThingDef d) => (d.BaseMarketValue - DebugOutputsEconomy.CostToMake(d, false)).ToString("F1"));
			expr_BF[8] = new TableDataGetter<ThingDef>("profit\nrate", (ThingDef d) => (d.recipeMaker == null) ? "-" : ((d.BaseMarketValue - DebugOutputsEconomy.CostToMake(d, false)) / DebugOutputsEconomy.WorkToProduceBest(d) * 10000f).ToString("F0"));
			expr_BF[9] = new TableDataGetter<ThingDef>("market value\ndefined", (ThingDef d) => d.statBases.Any((StatModifier st) => st.stat == StatDefOf.MarketValue).ToStringCheckBlank());
			expr_BF[10] = new TableDataGetter<ThingDef>("producible", (ThingDef d) => DebugOutputsEconomy.Producible(d).ToStringCheckBlank());
			expr_BF[11] = new TableDataGetter<ThingDef>("thing set\nmaker tags", (ThingDef d) => (!d.thingSetMakerTags.NullOrEmpty<string>()) ? d.thingSetMakerTags.ToCommaList(false) : string.Empty);
			expr_BF[12] = new TableDataGetter<ThingDef>("made\nfrom\nstuff", (ThingDef d) => d.MadeFromStuff.ToStringCheckBlank());
			expr_BF[13] = new TableDataGetter<ThingDef>("cost list", (ThingDef d) => DebugOutputsEconomy.CostListString(d, false, false));
			expr_BF[14] = new TableDataGetter<ThingDef>("recipes", (ThingDef d) => recipes(d));
			expr_BF[15] = new TableDataGetter<ThingDef>("work amount\nsources", (ThingDef d) => workAmountSources(d));
			DebugTables.MakeTablesDialog<ThingDef>(arg_333_0, expr_BF);
		}

		[Category("Economy"), DebugOutput]
		public static void ItemAccessibility()
		{
			IEnumerable<ThingDef> arg_129_0 = from x in ThingSetMakerUtility.allGeneratableItems
			orderby x.defName
			select x;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[6];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("1", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 1, Find.CurrentMap)) ? string.Empty : "✓");
			expr_2D[2] = new TableDataGetter<ThingDef>("10", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 10, Find.CurrentMap)) ? string.Empty : "✓");
			expr_2D[3] = new TableDataGetter<ThingDef>("100", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 100, Find.CurrentMap)) ? string.Empty : "✓");
			expr_2D[4] = new TableDataGetter<ThingDef>("1000", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 1000, Find.CurrentMap)) ? string.Empty : "✓");
			expr_2D[5] = new TableDataGetter<ThingDef>("10000", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 10000, Find.CurrentMap)) ? string.Empty : "✓");
			DebugTables.MakeTablesDialog<ThingDef>(arg_129_0, expr_2D);
		}

		[Category("Economy"), DebugOutput]
		public static void ThingSmeltProducts()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				Thing thing = ThingMaker.MakeThing(current, GenStuff.DefaultStuffFor(current));
				if (thing.SmeltProducts(1f).Any<Thing>())
				{
					stringBuilder.Append(thing.LabelCap + ": ");
					foreach (Thing current2 in thing.SmeltProducts(1f))
					{
						stringBuilder.Append(" " + current2.Label);
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString(), false);
		}

		[Category("Economy"), DebugOutput]
		public static void Recipes()
		{
			IEnumerable<RecipeDef> arg_229_0 = from d in DefDatabase<RecipeDef>.AllDefs
			where !d.products.NullOrEmpty<ThingDefCountClass>() && !d.ingredients.NullOrEmpty<IngredientCount>()
			select d;
			TableDataGetter<RecipeDef>[] expr_2E = new TableDataGetter<RecipeDef>[12];
			expr_2E[0] = new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName);
			expr_2E[1] = new TableDataGetter<RecipeDef>("work /w carry", (RecipeDef d) => DebugOutputsEconomy.TrueWorkWithCarryTime(d).ToString("F0"));
			expr_2E[2] = new TableDataGetter<RecipeDef>("work seconds", (RecipeDef d) => (DebugOutputsEconomy.TrueWorkWithCarryTime(d) / 60f).ToString("F0"));
			expr_2E[3] = new TableDataGetter<RecipeDef>("cheapest products value", (RecipeDef d) => DebugOutputsEconomy.CheapestProductsValue(d).ToString("F1"));
			expr_2E[4] = new TableDataGetter<RecipeDef>("cheapest ingredients value", (RecipeDef d) => DebugOutputsEconomy.CheapestIngredientValue(d).ToString("F1"));
			expr_2E[5] = new TableDataGetter<RecipeDef>("work value", (RecipeDef d) => DebugOutputsEconomy.WorkValueEstimate(d).ToString("F1"));
			expr_2E[6] = new TableDataGetter<RecipeDef>("profit raw", (RecipeDef d) => (DebugOutputsEconomy.CheapestProductsValue(d) - DebugOutputsEconomy.CheapestIngredientValue(d)).ToString("F1"));
			expr_2E[7] = new TableDataGetter<RecipeDef>("profit with work", (RecipeDef d) => (DebugOutputsEconomy.CheapestProductsValue(d) - DebugOutputsEconomy.WorkValueEstimate(d) - DebugOutputsEconomy.CheapestIngredientValue(d)).ToString("F1"));
			expr_2E[8] = new TableDataGetter<RecipeDef>("profit per work day", (RecipeDef d) => ((DebugOutputsEconomy.CheapestProductsValue(d) - DebugOutputsEconomy.CheapestIngredientValue(d)) * 60000f / DebugOutputsEconomy.TrueWorkWithCarryTime(d)).ToString("F0"));
			expr_2E[9] = new TableDataGetter<RecipeDef>("min skill", (RecipeDef d) => (!d.skillRequirements.NullOrEmpty<SkillRequirement>()) ? d.skillRequirements[0].Summary : string.Empty);
			expr_2E[10] = new TableDataGetter<RecipeDef>("cheapest stuff", (RecipeDef d) => (DebugOutputsEconomy.CheapestNonDerpStuff(d) == null) ? string.Empty : DebugOutputsEconomy.CheapestNonDerpStuff(d).defName);
			expr_2E[11] = new TableDataGetter<RecipeDef>("cheapest ingredients", (RecipeDef d) => (from pa in DebugOutputsEconomy.CheapestIngredients(d)
			select pa.First.defName + " x" + pa.Second).ToCommaList(false));
			DebugTables.MakeTablesDialog<RecipeDef>(arg_229_0, expr_2E);
		}

		private static bool Producible(BuildableDef b)
		{
			ThingDef d = b as ThingDef;
			TerrainDef terrainDef = b as TerrainDef;
			if (d != null)
			{
				if (DefDatabase<RecipeDef>.AllDefs.Any((RecipeDef r) => r.products.Any((ThingDefCountClass pr) => pr.thingDef == d)))
				{
					return true;
				}
				if (d.category == ThingCategory.Building && d.BuildableByPlayer)
				{
					return true;
				}
			}
			else if (terrainDef != null)
			{
				return terrainDef.BuildableByPlayer;
			}
			return false;
		}

		public static string CostListString(BuildableDef d, bool divideByVolume, bool starIfOnlyBuyable)
		{
			if (!DebugOutputsEconomy.Producible(d))
			{
				return string.Empty;
			}
			List<string> list = new List<string>();
			if (d.costList != null)
			{
				foreach (ThingDefCountClass current in d.costList)
				{
					float num = (float)current.count;
					if (divideByVolume)
					{
						num /= current.thingDef.VolumePerUnit;
					}
					string text = current.thingDef + " x" + num;
					if (starIfOnlyBuyable && DebugOutputsEconomy.RequiresBuying(current.thingDef))
					{
						text += "*";
					}
					list.Add(text);
				}
			}
			if (d.MadeFromStuff)
			{
				list.Add("stuff x" + d.costStuffCount);
			}
			return list.ToCommaList(false);
		}

		private static float TrueWorkWithCarryTime(RecipeDef d)
		{
			ThingDef stuffDef = DebugOutputsEconomy.CheapestNonDerpStuff(d);
			return (float)d.ingredients.Count * 90f + d.WorkAmountTotal(stuffDef) + 90f;
		}

		private static float CheapestIngredientValue(RecipeDef d)
		{
			float num = 0f;
			foreach (Pair<ThingDef, float> current in DebugOutputsEconomy.CheapestIngredients(d))
			{
				num += current.First.BaseMarketValue * current.Second;
			}
			return num;
		}

		[DebuggerHidden]
		private static IEnumerable<Pair<ThingDef, float>> CheapestIngredients(RecipeDef d)
		{
			foreach (IngredientCount ing in d.ingredients)
			{
				ThingDef thing = (from td in ing.filter.AllowedThingDefs
				where td != ThingDefOf.Meat_Human
				select td).MinBy((ThingDef td) => td.BaseMarketValue / td.VolumePerUnit);
				yield return new Pair<ThingDef, float>(thing, ing.GetBaseCount() / d.IngredientValueGetter.ValuePerUnitOf(thing));
			}
		}

		private static float WorkValueEstimate(RecipeDef d)
		{
			return DebugOutputsEconomy.TrueWorkWithCarryTime(d) * 0.01f;
		}

		private static ThingDef CheapestNonDerpStuff(RecipeDef d)
		{
			ThingDef productDef = d.products[0].thingDef;
			if (!productDef.MadeFromStuff)
			{
				return null;
			}
			return (from td in d.ingredients.First<IngredientCount>().filter.AllowedThingDefs
			where !productDef.IsWeapon || !PawnWeaponGenerator.IsDerpWeapon(productDef, td)
			select td).MinBy((ThingDef td) => td.BaseMarketValue / td.VolumePerUnit);
		}

		private static float CheapestProductsValue(RecipeDef d)
		{
			float num = 0f;
			foreach (ThingDefCountClass current in d.products)
			{
				num += current.thingDef.GetStatValueAbstract(StatDefOf.MarketValue, DebugOutputsEconomy.CheapestNonDerpStuff(d)) * (float)current.count;
			}
			return num;
		}

		private static string CostToMakeString(ThingDef d, bool real = false)
		{
			if (d.recipeMaker == null)
			{
				return "-";
			}
			return DebugOutputsEconomy.CostToMake(d, real).ToString("F1");
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
				foreach (ThingDefCountClass current in d.costList)
				{
					float num2 = 1f;
					if (real)
					{
						num2 = ((!DebugOutputsEconomy.RequiresBuying(current.thingDef)) ? 0.6f : 1.4f);
					}
					num += (float)current.count * DebugOutputsEconomy.CostToMake(current.thingDef, true) * num2;
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
				foreach (ThingDefCountClass current in def.costList)
				{
					if (DebugOutputsEconomy.RequiresBuying(current.thingDef))
					{
						return true;
					}
				}
				return false;
			}
			return !DefDatabase<ThingDef>.AllDefs.Any((ThingDef d) => d.plant != null && d.plant.harvestedThingDef == def && d.plant.Sowable);
		}

		public static float WorkToProduceBest(BuildableDef d)
		{
			float num = 3.40282347E+38f;
			if (d.StatBaseDefined(StatDefOf.WorkToMake))
			{
				num = d.GetStatValueAbstract(StatDefOf.WorkToMake, null);
			}
			if (d.StatBaseDefined(StatDefOf.WorkToBuild) && d.GetStatValueAbstract(StatDefOf.WorkToBuild, null) < num)
			{
				num = d.GetStatValueAbstract(StatDefOf.WorkToBuild, null);
			}
			foreach (RecipeDef current in DefDatabase<RecipeDef>.AllDefs)
			{
				if (current.workAmount > 0f && !current.products.NullOrEmpty<ThingDefCountClass>())
				{
					for (int i = 0; i < current.products.Count; i++)
					{
						if (current.products[i].thingDef == d && current.workAmount < num)
						{
							num = current.workAmount;
						}
					}
				}
			}
			if (num != 3.40282347E+38f)
			{
				return num;
			}
			return -1f;
		}
	}
}
