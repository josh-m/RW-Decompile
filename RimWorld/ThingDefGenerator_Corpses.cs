using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Corpses
	{
		private const float DaysToStartRot = 2.5f;

		private const float DaysToDessicate = 5f;

		private const float RotDamagePerDay = 2f;

		private const float DessicatedDamagePerDay = 0.7f;

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
					d.SetStatBaseValue(StatDefOf.Beauty, -150f);
					d.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
					d.alwaysHaulable = true;
					d.soundDrop = SoundDef.Named("Corpse_Drop");
					d.pathCost = 15;
					d.socialPropernessMatters = false;
					d.tradeability = Tradeability.Never;
					d.inspectorTabs = new List<Type>();
					d.inspectorTabs.Add(typeof(ITab_Pawn_Health));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Character));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Social));
					d.comps.Add(new CompProperties_Forbiddable());
					d.recipes = new List<RecipeDef>();
					if (raceDef.race.IsMechanoid)
					{
						d.recipes.Add(RecipeDefOf.RemoveMechanoidBodyPart);
					}
					else
					{
						d.recipes.Add(RecipeDefOf.RemoveBodyPart);
					}
					d.defName = raceDef.defName + "_Corpse";
					d.label = "CorpseLabel".Translate(new object[]
					{
						raceDef.label
					});
					d.description = "CorpseDesc".Translate(new object[]
					{
						raceDef.label
					});
					d.soundImpactDefault = raceDef.soundImpactDefault;
					d.SetStatBaseValue(StatDefOf.Flammability, raceDef.GetStatValueAbstract(StatDefOf.Flammability, null));
					d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)raceDef.BaseMaxHitPoints);
					d.SetStatBaseValue(StatDefOf.Mass, raceDef.statBases.GetStatOffsetFromList(StatDefOf.Mass));
					d.ingestible = new IngestibleProperties();
					IngestibleProperties ing = d.ingestible;
					ing.foodType = FoodTypeFlags.Corpse;
					ing.sourceDef = raceDef;
					ing.preferability = FoodPreferability.DesperateOnly;
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
					ing.nutrition = 1f;
					ing.maxNumToIngestAtOnce = 1;
					ing.ingestEffect = EffecterDefOf.EatMeat;
					ing.ingestSound = SoundDefOf.RawMeat_Eat;
					ing.specialThoughtDirect = raceDef.race.FleshType.ateDirect;
					if (raceDef.race.IsFlesh)
					{
						CompProperties_Rottable rottable = new CompProperties_Rottable();
						rottable.daysToRotStart = 2.5f;
						rottable.daysToDessicated = 5f;
						rottable.rotDamagePerDay = 2f;
						rottable.dessicatedDamagePerDay = 0.7f;
						d.comps.Add(rottable);
						CompProperties_SpawnerFilth corpseBileSpawner = new CompProperties_SpawnerFilth();
						corpseBileSpawner.filthDef = ThingDefOf.FilthCorpseBile;
						corpseBileSpawner.spawnCountOnSpawn = 0;
						corpseBileSpawner.spawnMtbHours = 0f;
						corpseBileSpawner.spawnRadius = 0.1f;
						corpseBileSpawner.spawnEveryDays = 1f;
						corpseBileSpawner.requiredRotStage = new RotStage?(RotStage.Rotting);
						d.comps.Add(corpseBileSpawner);
					}
					if (d.thingCategories == null)
					{
						d.thingCategories = new List<ThingCategoryDef>();
					}
					if (raceDef.race.Humanlike)
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, ThingCategoryDefOf.CorpsesHumanlike.defName);
					}
					else
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, raceDef.race.FleshType.corpseCategory.defName);
					}
					raceDef.race.corpseDef = d;
					yield return d;
				}
			}
		}
	}
}
