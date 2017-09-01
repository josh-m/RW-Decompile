using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnWeaponGenerator
	{
		private static List<ThingStuffPair> allWeaponPairs;

		private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

		public static void Reset()
		{
			Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && td.canBeSpawningInventory && !td.weaponTags.NullOrEmpty<string>();
			PawnWeaponGenerator.allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
			foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
			where isWeapon(td)
			select td)
			{
				float num = PawnWeaponGenerator.allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
				float num2 = thingDef.generateCommonality / num;
				if (num2 != 1f)
				{
					for (int i = 0; i < PawnWeaponGenerator.allWeaponPairs.Count; i++)
					{
						ThingStuffPair thingStuffPair = PawnWeaponGenerator.allWeaponPairs[i];
						if (thingStuffPair.thing == thingDef)
						{
							PawnWeaponGenerator.allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
						}
					}
				}
			}
		}

		public static void TryGenerateWeaponFor(Pawn pawn)
		{
			if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Count == 0)
			{
				return;
			}
			if (!pawn.RaceProps.ToolUser)
			{
				return;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return;
			}
			if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return;
			}
			float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
			for (int i = 0; i < PawnWeaponGenerator.allWeaponPairs.Count; i++)
			{
				ThingStuffPair w = PawnWeaponGenerator.allWeaponPairs[i];
				if (w.Price <= randomInRange)
				{
					if (pawn.kindDef.weaponTags.Any((string tag) => w.thing.weaponTags.Contains(tag)))
					{
						if (w.thing.generateAllowChance >= 1f || Rand.ValueSeeded(pawn.thingIDNumber ^ 28554824) <= w.thing.generateAllowChance)
						{
							PawnWeaponGenerator.workingWeapons.Add(w);
						}
					}
				}
			}
			if (PawnWeaponGenerator.workingWeapons.Count == 0)
			{
				return;
			}
			pawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
			ThingStuffPair thingStuffPair;
			if (PawnWeaponGenerator.workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out thingStuffPair))
			{
				ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
				PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
				pawn.equipment.AddEquipment(thingWithComps);
			}
			PawnWeaponGenerator.workingWeapons.Clear();
		}

		public static bool IsDerpWeapon(ThingDef thing, ThingDef stuff)
		{
			if (stuff == null)
			{
				return false;
			}
			if (thing.IsMeleeWeapon)
			{
				if (thing.Verbs.NullOrEmpty<VerbProperties>())
				{
					return false;
				}
				DamageArmorCategoryDef armorCategory = thing.Verbs[0].meleeDamageDef.armorCategory;
				if (armorCategory != null && armorCategory.multStat != null && stuff.GetStatValueAbstract(armorCategory.multStat, null) < 0.7f)
				{
					return true;
				}
			}
			return false;
		}

		public static float CheapestNonDerpPriceFor(ThingDef weaponDef)
		{
			float num = 9999999f;
			for (int i = 0; i < PawnWeaponGenerator.allWeaponPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = PawnWeaponGenerator.allWeaponPairs[i];
				if (thingStuffPair.thing == weaponDef && !PawnWeaponGenerator.IsDerpWeapon(thingStuffPair.thing, thingStuffPair.stuff) && thingStuffPair.Price < num)
				{
					num = thingStuffPair.Price;
				}
			}
			return num;
		}

		internal static void MakeTableWeaponPairs()
		{
			IEnumerable<ThingStuffPair> arg_153_0 = from p in PawnWeaponGenerator.allWeaponPairs
			orderby p.thing.defName descending
			select p;
			TableDataGetter<ThingStuffPair>[] expr_2D = new TableDataGetter<ThingStuffPair>[7];
			expr_2D[0] = new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName);
			expr_2D[1] = new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? string.Empty : p.stuff.defName);
			expr_2D[2] = new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString());
			expr_2D[3] = new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => p.Commonality.ToString("F5"));
			expr_2D[4] = new TableDataGetter<ThingStuffPair>("commMult", (ThingStuffPair p) => p.commonalityMultiplier.ToString("F5"));
			expr_2D[5] = new TableDataGetter<ThingStuffPair>("def-commonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F2"));
			expr_2D[6] = new TableDataGetter<ThingStuffPair>("derp", (ThingStuffPair p) => (!PawnWeaponGenerator.IsDerpWeapon(p.thing, p.stuff)) ? string.Empty : "D");
			DebugTables.MakeTablesDialog<ThingStuffPair>(arg_153_0, expr_2D);
		}

		internal static void MakeTableWeaponPairsByThing()
		{
			PawnApparelGenerator.MakeTablePairsByThing(PawnWeaponGenerator.allWeaponPairs);
		}
	}
}
