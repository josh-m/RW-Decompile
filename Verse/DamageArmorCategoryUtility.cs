using RimWorld;
using System;

namespace Verse
{
	public static class DamageArmorCategoryUtility
	{
		public static StatDef DeflectionStat(this DamageArmorCategory cat)
		{
			switch (cat)
			{
			case DamageArmorCategory.Blunt:
				return StatDefOf.ArmorRating_Blunt;
			case DamageArmorCategory.Sharp:
				return StatDefOf.ArmorRating_Sharp;
			case DamageArmorCategory.Heat:
				return StatDefOf.ArmorRating_Heat;
			case DamageArmorCategory.Electric:
				return StatDefOf.ArmorRating_Electric;
			default:
				return null;
			}
		}
	}
}
