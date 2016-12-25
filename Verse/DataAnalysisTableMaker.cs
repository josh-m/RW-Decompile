using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	internal static class DataAnalysisTableMaker
	{
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
			where d.category == ThingCategory.Item && d.BaseMarketValue > 0.001f && !d.isUnfinishedThing && !d.IsCorpse && !d.destroyOnDrop && d != ThingDefOf.Silver && !d.thingCategories.NullOrEmpty<ThingCategoryDef>()
			orderby d.thingCategories[0].defName, d.BaseMarketValue
			select d, list.ToArray());
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
				return d.GetStatValueAbstract(StatDefOf.WorkToMake, null);
			};
			IEnumerable<ThingDef> arg_148_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && d.BaseMarketValue > 0.01f && d.stackLimit > 1 == stackable
			orderby d.BaseMarketValue
			select d;
			TableDataGetter<ThingDef>[] expr_6E = new TableDataGetter<ThingDef>[6];
			expr_6E[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_6E[1] = new TableDataGetter<ThingDef>("base market value", (ThingDef d) => d.BaseMarketValue.ToString("F1"));
			expr_6E[2] = new TableDataGetter<ThingDef>("cost to make", (ThingDef d) => DataAnalysisTableMaker.CostToMakeRecursive(d).ToString("F1"));
			expr_6E[3] = new TableDataGetter<ThingDef>("work to make", (ThingDef d) => (d.recipeMaker == null) ? "-" : workAmountGetter(d).ToString("F1"));
			expr_6E[4] = new TableDataGetter<ThingDef>("profit", (ThingDef d) => (d.BaseMarketValue - DataAnalysisTableMaker.CostToMakeRecursive(d)).ToString("F1"));
			expr_6E[5] = new TableDataGetter<ThingDef>("profit rate", (ThingDef d) => (d.recipeMaker == null) ? "-" : ((d.BaseMarketValue - DataAnalysisTableMaker.CostToMakeRecursive(d)) / workAmountGetter(d) * 10000f).ToString("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_148_0, expr_6E);
		}

		private static float CostToMakeRecursive(ThingDef d)
		{
			if (d.recipeMaker == null)
			{
				return d.BaseMarketValue;
			}
			float num = 0f;
			if (d.costList != null)
			{
				foreach (ThingCount current in d.costList)
				{
					num += (float)current.count * DataAnalysisTableMaker.CostToMakeRecursive(current.thingDef);
				}
			}
			if (d.costStuffCount > 0)
			{
				ThingDef thingDef = GenStuff.DefaultStuffFor(d);
				num += (float)d.costStuffCount * thingDef.BaseMarketValue;
			}
			return num;
		}

		public static void DoTable_Recipes()
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
				foreach (ThingCount current in d.products)
				{
					num += current.thingDef.GetStatValueAbstract(StatDefOf.MarketValue, (!current.thingDef.MadeFromStuff) ? null : thingDef) * (float)current.count;
				}
				return num;
			};
			IEnumerable<RecipeDef> arg_187_0 = from d in DefDatabase<RecipeDef>.AllDefs
			where !d.products.NullOrEmpty<ThingCount>()
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

		public static void DoTable_HealingQualityPerMedicine()
		{
			List<float> list = new List<float>();
			list.Add(0.5f);
			list.AddRange(from d in DefDatabase<ThingDef>.AllDefs
			where typeof(Medicine).IsAssignableFrom(d.thingClass)
			select d.GetStatValueAbstract(StatDefOf.MedicalPotency, null));
			SkillNeed_Direct skillNeed_Direct = (SkillNeed_Direct)StatDefOf.BaseHealingQuality.skillNeedFactors[0];
			TableDataGetter<float>[] array = new TableDataGetter<float>[21];
			array[0] = new TableDataGetter<float>("potency", (float p) => p.ToStringPercent());
			for (int i = 0; i < 20; i++)
			{
				float factor = skillNeed_Direct.factorsPerLevel[i];
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

		public static void DoTable_Nutritions()
		{
			IEnumerable<ThingDef> arg_81_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.ingestible != null
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[2];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => d.ingestible.nutrition.ToStringPercentEmptyZero("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_81_0, expr_2D);
		}

		private static float RaceMeleeDpsEstimate(ThingDef race)
		{
			if (race.Verbs.NullOrEmpty<VerbProperties>())
			{
				return 0f;
			}
			IEnumerable<VerbProperties> source = from v in race.Verbs
			where (float)v.meleeDamageBaseAmount > 0.001f
			select v;
			float num = source.Sum((VerbProperties v) => (float)v.meleeDamageBaseAmount / (float)v.defaultCooldownTicks * 60f);
			return num / (float)source.Count<VerbProperties>();
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
				switch (d.RaceProps.trainableIntelligence)
				{
				case TrainableIntelligence.None:
					num *= 0.5f;
					break;
				case TrainableIntelligence.Simple:
					num *= 0.8f;
					break;
				case TrainableIntelligence.Intermediate:
					break;
				case TrainableIntelligence.Advanced:
					num += 250f;
					break;
				default:
					throw new InvalidOperationException();
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
			expr_7B[12] = new TableDataGetter<PawnKindDef>("train-int", (PawnKindDef d) => d.RaceProps.trainableIntelligence.GetLabel());
			expr_7B[13] = new TableDataGetter<PawnKindDef>("temps", (PawnKindDef d) => d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null).ToString("F0") + ".." + d.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null).ToString("F0"));
			expr_7B[14] = new TableDataGetter<PawnKindDef>("mateMtb", (PawnKindDef d) => d.RaceProps.mateMtbHours.ToStringEmptyZero("F0"));
			expr_7B[15] = new TableDataGetter<PawnKindDef>("nuzzMtb", (PawnKindDef d) => d.RaceProps.nuzzleMtbHours.ToStringEmptyZero("F0"));
			expr_7B[16] = new TableDataGetter<PawnKindDef>("mhChDam", (PawnKindDef d) => d.RaceProps.manhunterOnDamageChance.ToStringPercentEmptyZero("F2"));
			expr_7B[17] = new TableDataGetter<PawnKindDef>("mhChTam", (PawnKindDef d) => d.RaceProps.manhunterOnTameFailChance.ToStringPercentEmptyZero("F2"));
			DebugTables.MakeTablesDialog<PawnKindDef>(arg_345_0, expr_7B);
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

		public static void DoTable_PlantsBasics()
		{
			IEnumerable<ThingDef> arg_D5_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Plant
			select d;
			TableDataGetter<ThingDef>[] expr_2D = new TableDataGetter<ThingDef>[4];
			expr_2D[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_2D[1] = new TableDataGetter<ThingDef>("growDays", (ThingDef d) => d.plant.growDays.ToString("F2"));
			expr_2D[2] = new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => (d.ingestible == null) ? "-" : d.ingestible.nutrition.ToString("F2"));
			expr_2D[3] = new TableDataGetter<ThingDef>("nut/day", (ThingDef d) => (d.ingestible == null) ? "-" : (d.ingestible.nutrition / d.plant.growDays).ToString("F4"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_D5_0, expr_2D);
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
			list.Insert(0, new TableDataGetter<PawnKindDef>("animal", (PawnKindDef k) => k.defName));
			DebugTables.MakeTablesDialog<PawnKindDef>(from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race != null && d.RaceProps.Animal
			orderby d.defName
			select d, list.ToArray());
		}

		public static void DoTable_WeaponsRanged()
		{
			Func<ThingDef, int> damageGetter = (ThingDef d) => (d.Verbs[0].projectileDef == null) ? 0 : d.Verbs[0].projectileDef.projectile.damageAmountBase;
			Func<ThingDef, float> warmupGetter = (ThingDef d) => (float)d.Verbs[0].warmupTicks / 60f;
			Func<ThingDef, float> cooldownGetter = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, null);
			Func<ThingDef, int> burstShotsGetter = (ThingDef d) => d.Verbs[0].burstShotCount;
			Func<ThingDef, float> dpsRawGetter = delegate(ThingDef d)
			{
				int num = burstShotsGetter(d);
				float num2 = warmupGetter(d) + cooldownGetter(d);
				num2 += (float)(num - 1) * ((float)d.Verbs[0].ticksBetweenBurstShots / 60f);
				return (float)(damageGetter(d) * num) / num2;
			};
			Func<ThingDef, float> accTouchGetter = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyTouch, null);
			Func<ThingDef, float> accShortGetter = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyShort, null);
			Func<ThingDef, float> accMedGetter = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyMedium, null);
			Func<ThingDef, float> accLongGetter = (ThingDef d) => d.GetStatValueAbstract(StatDefOf.AccuracyLong, null);
			IEnumerable<ThingDef> arg_328_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsRangedWeapon
			select d;
			TableDataGetter<ThingDef>[] expr_15E = new TableDataGetter<ThingDef>[16];
			expr_15E[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_15E[1] = new TableDataGetter<ThingDef>("damage", (ThingDef d) => damageGetter(d).ToString());
			expr_15E[2] = new TableDataGetter<ThingDef>("warmup", (ThingDef d) => warmupGetter(d).ToString("F2"));
			expr_15E[3] = new TableDataGetter<ThingDef>("burst", (ThingDef d) => burstShotsGetter(d).ToString());
			expr_15E[4] = new TableDataGetter<ThingDef>("cooldown", (ThingDef d) => cooldownGetter(d).ToString("F2"));
			expr_15E[5] = new TableDataGetter<ThingDef>("dpsRaw", (ThingDef d) => dpsRawGetter(d).ToString("F3"));
			expr_15E[6] = new TableDataGetter<ThingDef>("accTouch", (ThingDef d) => accTouchGetter(d).ToStringPercent());
			expr_15E[7] = new TableDataGetter<ThingDef>("accShort", (ThingDef d) => accShortGetter(d).ToStringPercent());
			expr_15E[8] = new TableDataGetter<ThingDef>("accMed", (ThingDef d) => accMedGetter(d).ToStringPercent());
			expr_15E[9] = new TableDataGetter<ThingDef>("accLong", (ThingDef d) => accLongGetter(d).ToStringPercent());
			expr_15E[10] = new TableDataGetter<ThingDef>("dpsTouch", (ThingDef d) => (dpsRawGetter(d) * accTouchGetter(d)).ToString("F2"));
			expr_15E[11] = new TableDataGetter<ThingDef>("dpsShort", (ThingDef d) => (dpsRawGetter(d) * accShortGetter(d)).ToString("F2"));
			expr_15E[12] = new TableDataGetter<ThingDef>("dpsMed", (ThingDef d) => (dpsRawGetter(d) * accMedGetter(d)).ToString("F2"));
			expr_15E[13] = new TableDataGetter<ThingDef>("dpsLong", (ThingDef d) => (dpsRawGetter(d) * accLongGetter(d)).ToString("F2"));
			expr_15E[14] = new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, null).ToString("F0"));
			expr_15E[15] = new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, null).ToString("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_328_0, expr_15E);
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
			Func<Def, float> damageGetter = delegate(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					if (thingDef.race != null)
					{
						return thingDef.Verbs.Average((VerbProperties v) => (float)v.meleeDamageBaseAmount);
					}
					return thingDef.GetStatValueAbstract(StatDefOf.MeleeWeapon_DamageAmount, stuff);
				}
				else
				{
					HediffDef hediffDef = d as HediffDef;
					if (hediffDef != null)
					{
						return (float)hediffDef.CompPropsFor(typeof(HediffComp_VerbGiver)).verbs[0].meleeDamageBaseAmount;
					}
					return -1f;
				}
			};
			Func<Def, float> warmupGetter = delegate(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					return thingDef.Verbs.Average((VerbProperties v) => (float)v.warmupTicks) / 60f;
				}
				HediffDef hediffDef = d as HediffDef;
				if (hediffDef != null)
				{
					return (float)hediffDef.CompPropsFor(typeof(HediffComp_VerbGiver)).verbs[0].warmupTicks / 60f;
				}
				return -1f;
			};
			Func<Def, float> cooldownGetter = delegate(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef != null)
				{
					if (thingDef.race != null)
					{
						return thingDef.Verbs.Average((VerbProperties v) => (float)v.defaultCooldownTicks / 60f);
					}
					return thingDef.GetStatValueAbstract(StatDefOf.MeleeWeapon_Cooldown, stuff);
				}
				else
				{
					HediffDef hediffDef = d as HediffDef;
					if (hediffDef != null)
					{
						return (float)hediffDef.CompPropsFor(typeof(HediffComp_VerbGiver)).verbs[0].defaultCooldownTicks / 60f;
					}
					return -1f;
				}
			};
			Func<Def, float> dpsGetter = (Def d) => damageGetter(d) / (warmupGetter(d) + cooldownGetter(d));
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
			where h.CompPropsFor(typeof(HediffComp_VerbGiver)) != null
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
			IEnumerable<Def> arg_1E9_0 = enumerable;
			TableDataGetter<Def>[] expr_129 = new TableDataGetter<Def>[7];
			expr_129[0] = new TableDataGetter<Def>("defName", (Def d) => d.defName);
			expr_129[1] = new TableDataGetter<Def>("damage", (Def d) => damageGetter(d).ToString());
			expr_129[2] = new TableDataGetter<Def>("warmup", (Def d) => warmupGetter(d).ToString("F2"));
			expr_129[3] = new TableDataGetter<Def>("cooldown", (Def d) => cooldownGetter(d).ToString("F2"));
			expr_129[4] = new TableDataGetter<Def>("dps", (Def d) => dpsGetter(d).ToString("F2"));
			expr_129[5] = new TableDataGetter<Def>("mktval", (Def d) => marketValueGetter(d).ToString("F0"));
			expr_129[6] = new TableDataGetter<Def>("work", delegate(Def d)
			{
				ThingDef thingDef = d as ThingDef;
				if (thingDef == null)
				{
					return "-";
				}
				return thingDef.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0");
			});
			DebugTables.MakeTablesDialog<Def>(arg_1E9_0, expr_129);
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
			IEnumerable<ThingDef> arg_146_0 = from d in DefDatabase<ThingDef>.AllDefs
			where d.IsApparel
			select d;
			TableDataGetter<ThingDef>[] expr_3A = new TableDataGetter<ThingDef>[8];
			expr_3A[0] = new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName);
			expr_3A[1] = new TableDataGetter<ThingDef>("bodyParts", (ThingDef d) => GenText.ToSpaceList(from bp in d.apparel.bodyPartGroups
			select bp.defName));
			expr_3A[2] = new TableDataGetter<ThingDef>("layers", (ThingDef d) => GenText.ToSpaceList(from l in d.apparel.layers
			select l.ToString()));
			expr_3A[3] = new TableDataGetter<ThingDef>("tags", (ThingDef d) => GenText.ToSpaceList(from t in d.apparel.tags
			select t.ToString()));
			expr_3A[4] = new TableDataGetter<ThingDef>("insCold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff).ToString("F0"));
			expr_3A[5] = new TableDataGetter<ThingDef>("insHeat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff).ToString("F0"));
			expr_3A[6] = new TableDataGetter<ThingDef>("mktval", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, stuff).ToString("F0"));
			expr_3A[7] = new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_146_0, expr_3A);
		}
	}
}
