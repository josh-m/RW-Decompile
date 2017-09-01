using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Standard : ItemCollectionGenerator
	{
		private static List<ThingStuffPair> newCandidates = new List<ThingStuffPair>();

		private static List<ThingDef> allowedFiltered = new List<ThingDef>();

		private static Dictionary<ThingDef, List<ThingDef>> allowedStuff = new Dictionary<ThingDef, List<ThingDef>>();

		protected override ItemCollectionGeneratorParams RandomTestParams
		{
			get
			{
				ItemCollectionGeneratorParams randomTestParams = base.RandomTestParams;
				randomTestParams.count = Rand.RangeInclusive(5, 10);
				randomTestParams.techLevel = TechLevel.Transcendent;
				if (this.def == ItemCollectionGeneratorDefOf.AIPersonaCores || this.def == ItemCollectionGeneratorDefOf.Neurotrainers)
				{
					randomTestParams.totalMarketValue = 0f;
				}
				else
				{
					randomTestParams.totalMarketValue = Rand.Range(3000f, 8000f);
				}
				return randomTestParams;
			}
		}

		protected virtual IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			return this.def.allowedDefs;
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			TechLevel techLevel = parms.techLevel;
			int count = parms.count;
			float totalMarketValue = parms.totalMarketValue;
			IEnumerable<ThingDef> enumerable = this.AllowedDefs(parms);
			if (!enumerable.Any<ThingDef>())
			{
				return;
			}
			if (totalMarketValue > 0f)
			{
				List<ThingStuffPair> list = this.GenerateDefsWithPossibleTotalMarketValue(count, totalMarketValue, enumerable, techLevel, 100);
				for (int i = 0; i < list.Count; i++)
				{
					outThings.Add(ThingMaker.MakeThing(list[i].thing, list[i].stuff));
				}
				this.IncreaseStackCountsToMarketValue(outThings, totalMarketValue);
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					ThingDef thingDef = enumerable.RandomElement<ThingDef>();
					outThings.Add(ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef)));
				}
			}
			ItemCollectionGeneratorUtility.AssignRandomBaseGenItemQuality(outThings);
		}

		private List<ThingStuffPair> GenerateDefsWithPossibleTotalMarketValue(int count, float totalMarketValue, IEnumerable<ThingDef> allowed, TechLevel techLevel, int tries = 100)
		{
			List<ThingStuffPair> list = new List<ThingStuffPair>();
			ItemCollectionGenerator_Standard.allowedFiltered.Clear();
			ItemCollectionGenerator_Standard.allowedFiltered.AddRange(allowed);
			ItemCollectionGenerator_Standard.allowedFiltered.RemoveAll((ThingDef x) => x.techLevel > techLevel || (!x.MadeFromStuff && this.GetMinValue(new ThingStuffPair(x, null, 1f)) > totalMarketValue));
			ItemCollectionGenerator_Standard.allowedStuff.Clear();
			for (int i = 0; i < ItemCollectionGenerator_Standard.allowedFiltered.Count; i++)
			{
				ThingDef thingDef = ItemCollectionGenerator_Standard.allowedFiltered[i];
				if (thingDef.MadeFromStuff)
				{
					List<ThingDef> list2;
					if (!ItemCollectionGenerator_Standard.allowedStuff.TryGetValue(thingDef, out list2))
					{
						list2 = new List<ThingDef>();
						ItemCollectionGenerator_Standard.allowedStuff.Add(thingDef, list2);
					}
					foreach (ThingDef current in GenStuff.AllowedStuffsFor(thingDef))
					{
						if (current.stuffProps.commonality > 0f)
						{
							if (current.techLevel <= techLevel)
							{
								if (this.GetMinValue(new ThingStuffPair(thingDef, current, 1f)) <= totalMarketValue)
								{
									list2.Add(current);
								}
							}
						}
					}
				}
			}
			ItemCollectionGenerator_Standard.allowedFiltered.RemoveAll((ThingDef x) => x.MadeFromStuff && !ItemCollectionGenerator_Standard.allowedStuff[x].Any<ThingDef>());
			if (!ItemCollectionGenerator_Standard.allowedFiltered.Any<ThingDef>())
			{
				return list;
			}
			float num = 0f;
			for (int j = 0; j < tries; j++)
			{
				float num2 = 0f;
				float num3 = 0f;
				ItemCollectionGenerator_Standard.newCandidates.Clear();
				for (int k = 0; k < count; k++)
				{
					ThingDef thingDef2 = ItemCollectionGenerator_Standard.allowedFiltered.RandomElement<ThingDef>();
					ThingDef arg_204_0;
					if (thingDef2.MadeFromStuff)
					{
						arg_204_0 = ItemCollectionGenerator_Standard.allowedStuff[thingDef2].RandomElementByWeight((ThingDef x) => x.stuffProps.commonality);
					}
					else
					{
						arg_204_0 = null;
					}
					ThingDef stuff = arg_204_0;
					ThingStuffPair thingStuffPair = new ThingStuffPair(thingDef2, stuff, 1f);
					ItemCollectionGenerator_Standard.newCandidates.Add(thingStuffPair);
					num2 += this.GetMinValue(thingStuffPair);
					num3 += this.GetMaxValue(thingStuffPair);
				}
				float num4 = (num2 > totalMarketValue) ? (num2 - totalMarketValue) : 0f;
				float num5 = (num3 < totalMarketValue) ? (totalMarketValue - num3) : 0f;
				if (!list.Any<ThingStuffPair>() || num > num4 + num5)
				{
					list.Clear();
					list.AddRange(ItemCollectionGenerator_Standard.newCandidates);
					num = num4 + num5;
				}
				if (num <= 0f)
				{
					break;
				}
			}
			return list;
		}

		private void IncreaseStackCountsToMarketValue(List<Thing> things, float totalMarketValue)
		{
			float num = 0f;
			for (int i = 0; i < things.Count; i++)
			{
				num += things[i].MarketValue * (float)things[i].stackCount;
			}
			if (num >= totalMarketValue)
			{
				return;
			}
			float num2 = (totalMarketValue - num) / (float)things.Count;
			for (int j = 0; j < things.Count; j++)
			{
				float marketValue = things[j].MarketValue;
				int a = Mathf.FloorToInt(num2 / marketValue);
				int num3 = Mathf.Min(a, things[j].def.stackLimit - things[j].stackCount);
				if (num3 > 0)
				{
					things[j].stackCount += num3;
					num += marketValue * (float)num3;
				}
			}
			if (num >= totalMarketValue)
			{
				return;
			}
			for (int k = 0; k < things.Count; k++)
			{
				float marketValue2 = things[k].MarketValue;
				int a2 = Mathf.FloorToInt((totalMarketValue - num) / marketValue2);
				int num4 = Mathf.Min(a2, things[k].def.stackLimit - things[k].stackCount);
				if (num4 > 0)
				{
					things[k].stackCount += num4;
					num += marketValue2 * (float)num4;
				}
			}
		}

		private float GetMinValue(ThingStuffPair thingDef)
		{
			return thingDef.thing.GetStatValueAbstract(StatDefOf.MarketValue, thingDef.stuff);
		}

		private float GetMaxValue(ThingStuffPair thingDef)
		{
			return this.GetMinValue(thingDef) * (float)thingDef.thing.stackLimit;
		}
	}
}
