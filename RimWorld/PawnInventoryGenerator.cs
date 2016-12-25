using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnInventoryGenerator
	{
		public static void GenerateInventoryFor(Pawn p, PawnGenerationRequest request)
		{
			p.inventory.DestroyAll(DestroyMode.Vanish);
			for (int i = 0; i < p.kindDef.fixedInventory.Count; i++)
			{
				ThingCount thingCount = p.kindDef.fixedInventory[i];
				Thing thing = ThingMaker.MakeThing(thingCount.thingDef, null);
				thing.stackCount = thingCount.count;
				p.inventory.container.TryAdd(thing);
			}
			if (p.kindDef.inventoryOptions != null)
			{
				foreach (Thing current in p.kindDef.inventoryOptions.GenerateThings())
				{
					p.inventory.container.TryAdd(current);
				}
			}
			if (request.AllowFood)
			{
				PawnInventoryGenerator.GiveRandomFood(p);
			}
			PawnInventoryGenerator.GiveDrugsIfAddicted(p);
			PawnInventoryGenerator.GiveCombatEnhancingDrugs(p);
		}

		public static void GiveRandomFood(Pawn p)
		{
			if (p.kindDef.invNutrition > 0.001f)
			{
				ThingDef thingDef;
				if (p.kindDef.invFoodDef != null)
				{
					thingDef = p.kindDef.invFoodDef;
				}
				else
				{
					float value = Rand.Value;
					if (value < 0.5f)
					{
						thingDef = ThingDefOf.MealSimple;
					}
					else if ((double)value < 0.75)
					{
						thingDef = ThingDefOf.MealFine;
					}
					else
					{
						thingDef = ThingDefOf.MealSurvivalPack;
					}
				}
				Thing thing = ThingMaker.MakeThing(thingDef, null);
				thing.stackCount = GenMath.RoundRandom(p.kindDef.invNutrition / thingDef.ingestible.nutrition);
				p.inventory.container.TryAdd(thing);
			}
		}

		private static void GiveDrugsIfAddicted(Pawn p)
		{
			if (!p.RaceProps.Humanlike)
			{
				return;
			}
			IEnumerable<Hediff_Addiction> hediffs = p.health.hediffSet.GetHediffs<Hediff_Addiction>();
			foreach (Hediff_Addiction addiction in hediffs)
			{
				IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where(delegate(ThingDef x)
				{
					if (x.category != ThingCategory.Item)
					{
						return false;
					}
					if (p.Faction != null && x.techLevel > p.Faction.def.techLevel)
					{
						return false;
					}
					CompProperties_Drug compProperties = x.GetCompProperties<CompProperties_Drug>();
					return compProperties != null && compProperties.chemical != null && compProperties.chemical.addictionHediff == addiction.def;
				});
				ThingDef def;
				if (source.TryRandomElement(out def))
				{
					int stackCount = Rand.RangeInclusive(2, 5);
					Thing thing = ThingMaker.MakeThing(def, null);
					thing.stackCount = stackCount;
					p.inventory.container.TryAdd(thing);
				}
			}
		}

		private static void GiveCombatEnhancingDrugs(Pawn pawn)
		{
			if (Rand.Value >= pawn.kindDef.combatEnhancingDrugsChance)
			{
				return;
			}
			if (pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0)
			{
				return;
			}
			for (int i = 0; i < pawn.inventory.container.Count; i++)
			{
				CompDrug compDrug = pawn.inventory.container[i].TryGetComp<CompDrug>();
				if (compDrug != null && compDrug.Props.isCombatEnhancingDrug)
				{
					return;
				}
			}
			int randomInRange = pawn.kindDef.combatEnhancingDrugsCount.RandomInRange;
			if (randomInRange <= 0)
			{
				return;
			}
			IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where(delegate(ThingDef x)
			{
				if (x.category != ThingCategory.Item)
				{
					return false;
				}
				if (pawn.Faction != null && x.techLevel > pawn.Faction.def.techLevel)
				{
					return false;
				}
				CompProperties_Drug compProperties = x.GetCompProperties<CompProperties_Drug>();
				return compProperties != null && compProperties.isCombatEnhancingDrug;
			});
			for (int j = 0; j < randomInRange; j++)
			{
				ThingDef def;
				if (!source.TryRandomElement(out def))
				{
					break;
				}
				pawn.inventory.container.TryAdd(ThingMaker.MakeThing(def, null));
			}
		}
	}
}
