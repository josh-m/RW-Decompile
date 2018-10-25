using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Corpses
	{
		private const float DaysToStartRot = 2.5f;

		private const float DaysToDessicate = 5f;

		private const float RotDamagePerDay = 2f;

		private const float DessicatedDamagePerDay = 0.7f;

		private const float ButcherProductsMarketValueFactor = 0.6f;

		[DebuggerHidden]
		public static IEnumerable<ThingDef> ImpliedCorpseDefs()
		{
			foreach (ThingDef raceDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
			{
				if (raceDef.category == ThingCategory.Pawn)
				{
					ThingDef d = new ThingDef();
					d.category = ThingCategory.Item;
					d.thingClass = typeof(Corpse);
					d.selectable = true;
					d.tickerType = TickerType.Rare;
					d.altitudeLayer = AltitudeLayer.ItemImportant;
					d.scatterableOnMapGen = false;
					d.SetStatBaseValue(StatDefOf.Beauty, -50f);
					d.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
					d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
					d.alwaysHaulable = true;
					d.soundDrop = SoundDefOf.Corpse_Drop;
					d.pathCost = 15;
					d.socialPropernessMatters = false;
					d.tradeability = Tradeability.None;
					d.inspectorTabs = new List<Type>();
					d.inspectorTabs.Add(typeof(ITab_Pawn_Health));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Character));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Social));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Log));
					d.comps.Add(new CompProperties_Forbiddable());
					d.recipes = new List<RecipeDef>();
					if (!raceDef.race.IsMechanoid)
					{
						d.recipes.Add(RecipeDefOf.RemoveBodyPart);
					}
					d.defName = "Corpse_" + raceDef.defName;
					d.label = "CorpseLabel".Translate(raceDef.label);
					d.description = "CorpseDesc".Translate(raceDef.label);
					d.soundImpactDefault = raceDef.soundImpactDefault;
					d.SetStatBaseValue(StatDefOf.MarketValue, ThingDefGenerator_Corpses.CalculateMarketValue(raceDef));
					d.SetStatBaseValue(StatDefOf.Flammability, raceDef.GetStatValueAbstract(StatDefOf.Flammability, null));
					d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)raceDef.BaseMaxHitPoints);
					d.SetStatBaseValue(StatDefOf.Mass, raceDef.statBases.GetStatOffsetFromList(StatDefOf.Mass));
					d.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
					d.modContentPack = raceDef.modContentPack;
					d.ingestible = new IngestibleProperties();
					d.ingestible.parent = d;
					IngestibleProperties ing = d.ingestible;
					ing.foodType = FoodTypeFlags.Corpse;
					ing.sourceDef = raceDef;
					ing.preferability = ((!raceDef.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
					ing.maxNumToIngestAtOnce = 1;
					ing.ingestEffect = EffecterDefOf.EatMeat;
					ing.ingestSound = SoundDefOf.RawMeat_Eat;
					ing.specialThoughtDirect = raceDef.race.FleshType.ateDirect;
					if (raceDef.race.IsFlesh)
					{
						CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
						compProperties_Rottable.daysToRotStart = 2.5f;
						compProperties_Rottable.daysToDessicated = 5f;
						compProperties_Rottable.rotDamagePerDay = 2f;
						compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
						d.comps.Add(compProperties_Rottable);
						CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
						compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
						compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
						compProperties_SpawnerFilth.spawnMtbHours = 0f;
						compProperties_SpawnerFilth.spawnRadius = 0.1f;
						compProperties_SpawnerFilth.spawnEveryDays = 1f;
						compProperties_SpawnerFilth.requiredRotStage = new RotStage?(RotStage.Rotting);
						d.comps.Add(compProperties_SpawnerFilth);
					}
					if (d.thingCategories == null)
					{
						d.thingCategories = new List<ThingCategoryDef>();
					}
					if (raceDef.race.Humanlike)
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, ThingCategoryDefOf.CorpsesHumanlike.defName, d);
					}
					else
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, raceDef.race.FleshType.corpseCategory.defName, d);
					}
					raceDef.race.corpseDef = d;
					yield return d;
				}
			}
		}

		private static float CalculateMarketValue(ThingDef raceDef)
		{
			float num = 0f;
			if (raceDef.race.meatDef != null)
			{
				int num2 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.MeatAmount, null));
				num += (float)num2 * raceDef.race.meatDef.GetStatValueAbstract(StatDefOf.MarketValue, null);
			}
			if (raceDef.race.leatherDef != null)
			{
				int num3 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.LeatherAmount, null));
				num += (float)num3 * raceDef.race.leatherDef.GetStatValueAbstract(StatDefOf.MarketValue, null);
			}
			if (raceDef.butcherProducts != null)
			{
				for (int i = 0; i < raceDef.butcherProducts.Count; i++)
				{
					num += raceDef.butcherProducts[i].thingDef.BaseMarketValue * (float)raceDef.butcherProducts[i].count;
				}
			}
			return num * 0.6f;
		}
	}
}
