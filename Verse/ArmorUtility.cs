using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class ArmorUtility
	{
		public static int GetPostArmorDamage(Pawn pawn, int amountInt, BodyPartRecord part, DamageDef damageDef)
		{
			float num = (float)amountInt;
			if (damageDef.armorCategory == null)
			{
				return amountInt;
			}
			StatDef deflectionStat = damageDef.armorCategory.deflectionStat;
			if (pawn.apparel != null)
			{
				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					Apparel apparel = wornApparel[i];
					if (apparel.def.apparel.CoversBodyPart(part))
					{
						ArmorUtility.ApplyArmor(ref num, apparel.GetStatValue(deflectionStat, true), apparel, damageDef);
						if (num < 0.001f)
						{
							return 0;
						}
					}
				}
			}
			ArmorUtility.ApplyArmor(ref num, pawn.GetStatValue(deflectionStat, true), null, damageDef);
			return GenMath.RoundRandom(num);
		}

		private static void ApplyArmor(ref float damAmount, float armorRating, Thing armorThing, DamageDef damageDef)
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
				float f = damAmount * 0.25f;
				armorThing.TakeDamage(new DamageInfo(damageDef, GenMath.RoundRandom(f), -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
			}
			damAmount -= num3;
		}
	}
}
