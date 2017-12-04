using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Standard : ItemCollectionGenerator
	{
		protected virtual IEnumerable<ThingDef> AllowedDefs(ItemCollectionGeneratorParams parms)
		{
			return this.def.allowedDefs;
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = (!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value;
			int? count = parms.count;
			int num = (!count.HasValue) ? Rand.RangeInclusive(5, 10) : count.Value;
			float? totalMarketValue = parms.totalMarketValue;
			IEnumerable<ThingDef> enumerable = this.AllowedDefs(parms);
			if (parms.extraAllowedDefs != null)
			{
				enumerable = enumerable.Concat(parms.extraAllowedDefs);
			}
			if (!enumerable.Any<ThingDef>())
			{
				return;
			}
			if (totalMarketValue.HasValue)
			{
				List<ThingStuffPair> list = ItemCollectionGeneratorByTotalValueUtility.GenerateDefsWithPossibleTotalValue(num, totalMarketValue.Value, enumerable, techLevel2, (Thing x) => x.MarketValue, new Func<ThingStuffPair, float>(this.GetMinValue), new Func<ThingStuffPair, float>(this.GetMaxValue), null, 100);
				for (int i = 0; i < list.Count; i++)
				{
					outThings.Add(ThingMaker.MakeThing(list[i].thing, list[i].stuff));
				}
				ItemCollectionGeneratorByTotalValueUtility.IncreaseStackCountsToTotalValue(outThings, totalMarketValue.Value, (Thing x) => x.MarketValue);
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					ThingDef thingDef = enumerable.RandomElement<ThingDef>();
					outThings.Add(ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef)));
				}
			}
			ItemCollectionGeneratorUtility.AssignRandomBaseGenItemQuality(outThings);
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
