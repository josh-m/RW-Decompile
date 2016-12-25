using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class PawnWeaponGenerator
	{
		private static List<ThingStuffPair> potentialWeapons;

		private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

		public static void Reset()
		{
			PawnWeaponGenerator.potentialWeapons = ThingStuffPair.AllWith((ThingDef td) => td.equipmentType == EquipmentType.Primary && td.canBeSpawningInventory && !td.weaponTags.NullOrEmpty<string>());
		}

		public static float CheapestNonDerpPriceFor(ThingDef weaponDef)
		{
			float num = 9999999f;
			for (int i = 0; i < PawnWeaponGenerator.potentialWeapons.Count; i++)
			{
				ThingStuffPair thingStuffPair = PawnWeaponGenerator.potentialWeapons[i];
				if (thingStuffPair.thing == weaponDef && !PawnWeaponGenerator.IsDerpWeapon(thingStuffPair.thing, thingStuffPair.stuff) && thingStuffPair.Price < num)
				{
					num = thingStuffPair.Price;
				}
			}
			return num;
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
			for (int i = 0; i < PawnWeaponGenerator.potentialWeapons.Count; i++)
			{
				ThingStuffPair w = PawnWeaponGenerator.potentialWeapons[i];
				if (w.Price <= randomInRange)
				{
					if (pawn.kindDef.weaponTags.Any((string tag) => w.thing.weaponTags.Contains(tag)))
					{
						PawnWeaponGenerator.workingWeapons.Add(w);
					}
				}
			}
			if (PawnWeaponGenerator.workingWeapons.Count == 0)
			{
				return;
			}
			pawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
			ThingStuffPair thingStuffPair = PawnWeaponGenerator.workingWeapons.RandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price);
			ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
			PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
			pawn.equipment.AddEquipment(thingWithComps);
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
				DamageArmorCategory armorCategory = thing.Verbs[0].meleeDamageDef.armorCategory;
				if (armorCategory == DamageArmorCategory.Sharp && stuff.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier, null) < 0.7f)
				{
					return true;
				}
				if (armorCategory == DamageArmorCategory.Blunt && stuff.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier, null) < 0.7f)
				{
					return true;
				}
			}
			return false;
		}

		internal static void LogGenerationData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("All potential weapons:");
			foreach (ThingStuffPair current in PawnWeaponGenerator.potentialWeapons)
			{
				stringBuilder.Append(current.ToString());
				if (PawnWeaponGenerator.IsDerpWeapon(current.thing, current.stuff))
				{
					stringBuilder.Append(" - (DERP)");
				}
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
