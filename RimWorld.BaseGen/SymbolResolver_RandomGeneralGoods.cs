using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_RandomGeneralGoods : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			foreach (IntVec3 current in rp.rect)
			{
				if (current.Standable(map))
				{
					float value = Rand.Value;
					if (value >= 0.28f)
					{
						if (value < 0.6f)
						{
							this.SpawnRandomMeal(current, rp.faction);
						}
						else if (value < 0.85f)
						{
							this.SpawnRandomRawFood(current);
						}
						else if (value < 0.925f)
						{
							this.SpawnRandomMedicine(current, rp.faction);
						}
						else
						{
							this.SpawnRandomDrug(current, rp.faction);
						}
					}
				}
			}
		}

		private void SpawnRandomMeal(IntVec3 c, Faction faction)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef;
			if (faction != null && faction.def.techLevel.IsNeolithicOrWorse())
			{
				thingDef = ThingDefOf.Pemmican;
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
			thing.stackCount = Rand.RangeInclusive(thingDef.stackLimit / 2, thingDef.stackLimit);
			GenSpawn.Spawn(thing, c, map);
			thing.SetForbidden(true, false);
		}

		private void SpawnRandomRawFood(IntVec3 c)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef;
			if (!(from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.IsNutritionGivingIngestible && !x.IsCorpse && ThingDefOf.Human.race.CanEverEat(x) && x.ingestible.preferability < FoodPreferability.MealAwful
			select x).TryRandomElement(out thingDef))
			{
				return;
			}
			int num = thingDef.stackLimit;
			if (thingDef.HasComp(typeof(CompHatcher)))
			{
				num = Mathf.Min(num, 10);
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			thing.stackCount = Rand.RangeInclusive(1, num);
			GenSpawn.Spawn(thing, c, map);
			thing.SetForbidden(true, false);
		}

		private void SpawnRandomMedicine(IntVec3 c, Faction faction)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef herbalMedicine;
			if (faction != null && faction.def.techLevel.IsNeolithicOrWorse())
			{
				herbalMedicine = ThingDefOf.HerbalMedicine;
			}
			else if (!(from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.IsMedicine
			select x).TryRandomElement(out herbalMedicine))
			{
				return;
			}
			Thing thing = ThingMaker.MakeThing(herbalMedicine, null);
			thing.stackCount = Rand.RangeInclusive(1, Mathf.Min(herbalMedicine.stackLimit, 20));
			GenSpawn.Spawn(thing, c, map);
			thing.SetForbidden(true, false);
		}

		private void SpawnRandomDrug(IntVec3 c, Faction faction)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef;
			if (!(from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.IsDrug && (faction == null || x.techLevel <= faction.def.techLevel)
			select x).TryRandomElement(out thingDef))
			{
				return;
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			thing.stackCount = Rand.RangeInclusive(1, Mathf.Min(thingDef.stackLimit, 25));
			GenSpawn.Spawn(thing, c, map);
			thing.SetForbidden(true, false);
		}
	}
}
