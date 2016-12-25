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

		private const float RotDamagePerDay = 2.5f;

		private const float DessicatedDamagePerDay = 0.75f;

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
					d.category = ThingCategory.Item;
					d.selectable = true;
					d.tickerType = TickerType.Rare;
					d.altitudeLayer = AltitudeLayer.ItemImportant;
					d.canMakeOnMapGen = false;
					d.SetStatBaseValue(StatDefOf.Flammability, 1f);
					d.soundImpactDefault = SoundDef.Named("BulletImpactFlesh");
					d.SetStatBaseValue(StatDefOf.Beauty, -150f);
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
					CompProperties_Rottable rottable = new CompProperties_Rottable();
					d.defName = raceDef.defName + "_Corpse";
					d.label = "CorpseLabel".Translate(new object[]
					{
						raceDef.label
					});
					d.description = "CorpseDesc".Translate(new object[]
					{
						raceDef.label
					});
					d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)raceDef.BaseMaxHitPoints);
					d.ingestible = new IngestibleProperties();
					IngestibleProperties ing = d.ingestible;
					ing.foodType = FoodTypeFlags.Corpse;
					ing.sourceDef = raceDef;
					ing.preferability = FoodPreferability.DesperateOnly;
					ing.nutrition = 1f;
					ing.maxNumToIngestAtOnce = 1;
					ing.ingestEffect = EffecterDefOf.EatMeat;
					ing.ingestSound = SoundDefOf.RawMeat_Eat;
					if (raceDef.race.fleshType == FleshType.Insectoid)
					{
						ing.specialThoughtDirect = ThoughtDefOf.AteInsectMeatDirect;
					}
					if (raceDef.race.IsFlesh)
					{
						rottable.compClass = typeof(CompRottable);
						rottable.daysToRotStart = 2.5f;
						rottable.daysToDessicated = 5f;
						rottable.rotDamagePerDay = 2.5f;
						rottable.dessicatedDamagePerDay = 0.75f;
						d.comps.Add(rottable);
					}
					if (d.thingCategories == null)
					{
						d.thingCategories = new List<ThingCategoryDef>();
					}
					if (raceDef.race.Humanlike)
					{
						CrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, "CorpsesHumanlike");
					}
					else if (!raceDef.race.IsFlesh)
					{
						CrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, "CorpsesMechanoid");
					}
					else if (raceDef.race.fleshType == FleshType.Insectoid)
					{
						CrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, "CorpsesInsect");
					}
					else
					{
						CrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, "CorpsesAnimal");
					}
					raceDef.race.corpseDef = d;
					yield return d;
				}
			}
		}
	}
}
