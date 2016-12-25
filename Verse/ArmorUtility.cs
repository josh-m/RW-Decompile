using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class ArmorUtility
	{
		public static int GetAfterArmorDamage(Pawn pawn, int amountInt, BodyPartRecord part, DamageDef damageDef)
		{
			float num = (float)amountInt;
			if (damageDef.armorCategory == DamageArmorCategory.IgnoreArmor)
			{
				return amountInt;
			}
			StatDef stat = damageDef.armorCategory.DeflectionStat();
			if (pawn.apparel != null)
			{
				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					Apparel apparel = wornApparel[i];
					if (apparel.def.apparel.CoversBodyPart(part))
					{
						ArmorUtility.ApplyArmor(ref num, apparel.GetStatValue(stat, true), apparel, damageDef);
						if (num < 0.001f)
						{
							return 0;
						}
					}
				}
			}
			ArmorUtility.ApplyArmor(ref num, pawn.GetStatValue(stat, true), null, damageDef);
			return Mathf.RoundToInt(num);
		}

		public static void ApplyArmor(ref float damAmount, float armorRating, Thing armorThing, DamageDef damageDef)
		{
			float num;
			float num2;
			if ((double)armorRating <= 0.5)
			{
				num = armorRating;
				num2 = 0f;
			}
			else if (armorRating < 1f)
			{
				num = 0.5f;
				num2 = armorRating - 0.5f;
			}
			else
			{
				num = 0.5f + (armorRating - 1f) * 0.25f;
				num2 = 0.5f + (armorRating - 1f) * 0.25f;
			}
			if (num > 0.9f)
			{
				num = 0.9f;
			}
			if (num2 > 0.9f)
			{
				num2 = 0.9f;
			}
			float num3;
			if (Rand.Value < num2)
			{
				num3 = damAmount;
			}
			else
			{
				num3 = damAmount * num;
			}
			if (armorThing != null)
			{
				armorThing.TakeDamage(new DamageInfo(damageDef, GenMath.RoundRandom(num3), -1f, null, null, null));
			}
			damAmount -= num3;
		}
	}
}
