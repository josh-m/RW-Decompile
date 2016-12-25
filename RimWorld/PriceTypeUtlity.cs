using System;
using System.Collections;
using UnityEngine;

namespace RimWorld
{
	public static class PriceTypeUtlity
	{
		public static float PriceMultiplier(this PriceType pType)
		{
			switch (pType)
			{
			case PriceType.VeryCheap:
				return 0.4f;
			case PriceType.Cheap:
				return 0.7f;
			case PriceType.Normal:
				return 1f;
			case PriceType.Expensive:
				return 2f;
			case PriceType.Exorbitant:
				return 5f;
			default:
				return -1f;
			}
		}

		public static PriceType ClosestPriceType(float priceFactor)
		{
			float num = 99999f;
			PriceType priceType = PriceType.Undefined;
			using (IEnumerator enumerator = Enum.GetValues(typeof(PriceType)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PriceType priceType2 = (PriceType)((byte)enumerator.Current);
					float num2 = Mathf.Abs(priceFactor - priceType2.PriceMultiplier());
					if (num2 < num)
					{
						num = num2;
						priceType = priceType2;
					}
				}
			}
			if (priceType == PriceType.Undefined)
			{
				priceType = PriceType.VeryCheap;
			}
			return priceType;
		}
	}
}
