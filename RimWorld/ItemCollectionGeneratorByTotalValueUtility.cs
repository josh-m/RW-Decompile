using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ItemCollectionGeneratorByTotalValueUtility
	{
		private static List<ThingStuffPair> newCandidates = new List<ThingStuffPair>();

		private static List<ThingDef> allowedFiltered = new List<ThingDef>();

		private static Dictionary<ThingDef, List<ThingDef>> allowedStuff = new Dictionary<ThingDef, List<ThingDef>>();

		public static List<ThingStuffPair> GenerateDefsWithPossibleTotalValue(int count, float totalValue, IEnumerable<ThingDef> allowed, TechLevel techLevel, Func<Thing, float> getValue, Func<ThingStuffPair, float> getMinValue, Func<ThingStuffPair, float> getMaxValue, Func<ThingDef, float> weightSelector = null, int tries = 100)
		{
			List<ThingStuffPair> list = new List<ThingStuffPair>();
			ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.Clear();
			ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.AddRange(allowed);
			ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.RemoveAll((ThingDef x) => x.techLevel > techLevel || (!x.MadeFromStuff && getMinValue(new ThingStuffPair(x, null, 1f)) > totalValue));
			ItemCollectionGeneratorByTotalValueUtility.allowedStuff.Clear();
			for (int i = 0; i < ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.Count; i++)
			{
				ThingDef thingDef = ItemCollectionGeneratorByTotalValueUtility.allowedFiltered[i];
				if (thingDef.MadeFromStuff)
				{
					List<ThingDef> list2;
					if (!ItemCollectionGeneratorByTotalValueUtility.allowedStuff.TryGetValue(thingDef, out list2))
					{
						list2 = new List<ThingDef>();
						ItemCollectionGeneratorByTotalValueUtility.allowedStuff.Add(thingDef, list2);
					}
					foreach (ThingDef current in GenStuff.AllowedStuffsFor(thingDef))
					{
						if (current.stuffProps.commonality > 0f)
						{
							if (current.techLevel <= techLevel)
							{
								if (getMinValue(new ThingStuffPair(thingDef, current, 1f)) <= totalValue)
								{
									list2.Add(current);
								}
							}
						}
					}
				}
			}
			ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.RemoveAll((ThingDef x) => x.MadeFromStuff && !ItemCollectionGeneratorByTotalValueUtility.allowedStuff[x].Any<ThingDef>());
			if (!ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.Any<ThingDef>())
			{
				return list;
			}
			float num = 0f;
			for (int j = 0; j < tries; j++)
			{
				float num2 = 0f;
				float num3 = 0f;
				ItemCollectionGeneratorByTotalValueUtility.newCandidates.Clear();
				for (int k = 0; k < count; k++)
				{
					ThingDef thingDef2;
					if (weightSelector != null)
					{
						thingDef2 = ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.RandomElementByWeight(weightSelector);
					}
					else
					{
						thingDef2 = ItemCollectionGeneratorByTotalValueUtility.allowedFiltered.RandomElement<ThingDef>();
					}
					ThingDef arg_221_0;
					if (thingDef2.MadeFromStuff)
					{
						arg_221_0 = ItemCollectionGeneratorByTotalValueUtility.allowedStuff[thingDef2].RandomElementByWeight((ThingDef x) => x.stuffProps.commonality);
					}
					else
					{
						arg_221_0 = null;
					}
					ThingDef stuff = arg_221_0;
					ThingStuffPair thingStuffPair = new ThingStuffPair(thingDef2, stuff, 1f);
					ItemCollectionGeneratorByTotalValueUtility.newCandidates.Add(thingStuffPair);
					num2 += getMinValue(thingStuffPair);
					num3 += getMaxValue(thingStuffPair);
				}
				float num4 = (num2 > totalValue) ? (num2 - totalValue) : 0f;
				float num5 = (num3 < totalValue) ? (totalValue - num3) : 0f;
				if (!list.Any<ThingStuffPair>() || num > num4 + num5)
				{
					list.Clear();
					list.AddRange(ItemCollectionGeneratorByTotalValueUtility.newCandidates);
					num = num4 + num5;
				}
				if (num <= 0f)
				{
					break;
				}
			}
			return list;
		}

		public static void IncreaseStackCountsToTotalValue(List<Thing> things, float totalValue, Func<Thing, float> getValue)
		{
			float num = 0f;
			for (int i = 0; i < things.Count; i++)
			{
				num += getValue(things[i]) * (float)things[i].stackCount;
			}
			if (num >= totalValue)
			{
				return;
			}
			float num2 = (totalValue - num) / (float)things.Count;
			for (int j = 0; j < things.Count; j++)
			{
				float num3 = getValue(things[j]);
				int a = Mathf.FloorToInt(num2 / num3);
				int num4 = Mathf.Min(a, things[j].def.stackLimit - things[j].stackCount);
				if (num4 > 0)
				{
					things[j].stackCount += num4;
					num += num3 * (float)num4;
				}
			}
			if (num >= totalValue)
			{
				return;
			}
			for (int k = 0; k < things.Count; k++)
			{
				float num5 = getValue(things[k]);
				int a2 = Mathf.FloorToInt((totalValue - num) / num5);
				int num6 = Mathf.Min(a2, things[k].def.stackLimit - things[k].stackCount);
				if (num6 > 0)
				{
					things[k].stackCount += num6;
					num += num5 * (float)num6;
				}
			}
		}
	}
}
