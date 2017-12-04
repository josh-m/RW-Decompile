using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_Food : ItemCollectionGenerator
	{
		private static List<ThingDef> food = new List<ThingDef>();

		public static void Reset()
		{
			ItemCollectionGenerator_Food.food.Clear();
			ItemCollectionGenerator_Food.food.AddRange(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsNutritionGivingIngestible && !x.HasComp(typeof(CompHatcher)) && !x.IsDrug
			select x);
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int? count = parms.count;
			int count2 = (!count.HasValue) ? Rand.RangeInclusive(3, 6) : count.Value;
			float? totalNutrition = parms.totalNutrition;
			float totalValue = (!totalNutrition.HasValue) ? Rand.Range(5f, 10f) : totalNutrition.Value;
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = (!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value;
			bool? nonHumanEdibleFoodAllowed = parms.nonHumanEdibleFoodAllowed;
			bool flag = nonHumanEdibleFoodAllowed.HasValue && parms.nonHumanEdibleFoodAllowed.Value;
			IEnumerable<ThingDef> arg_FC_0;
			if (flag)
			{
				arg_FC_0 = ItemCollectionGenerator_Food.food;
			}
			else
			{
				arg_FC_0 = from x in ItemCollectionGenerator_Food.food
				where x.ingestible.HumanEdible
				select x;
			}
			IEnumerable<ThingDef> enumerable = arg_FC_0;
			if (!flag)
			{
				enumerable = from x in enumerable
				where x.ingestible.preferability > FoodPreferability.NeverForNutrition
				select x;
			}
			FoodPreferability? minPreferability = parms.minPreferability;
			if (minPreferability.HasValue)
			{
				enumerable = from x in enumerable
				where x.ingestible.preferability >= parms.minPreferability.Value
				select x;
			}
			if (!enumerable.Any<ThingDef>())
			{
				return;
			}
			int numMeats = (from x in enumerable
			where x.IsMeat
			select x).Count<ThingDef>();
			int numLeathers = (from x in enumerable
			where x.IsLeather
			select x).Count<ThingDef>();
			Func<ThingDef, float> weightSelector = (ThingDef x) => ItemCollectionGeneratorUtility.AdjustedSelectionWeight(x, numMeats, numLeathers);
			List<ThingStuffPair> list = ItemCollectionGeneratorByTotalValueUtility.GenerateDefsWithPossibleTotalValue(count2, totalValue, enumerable, techLevel2, (Thing x) => x.def.ingestible.nutrition, (ThingStuffPair x) => x.thing.ingestible.nutrition, (ThingStuffPair x) => x.thing.ingestible.nutrition * (float)x.thing.stackLimit, weightSelector, 100);
			for (int i = 0; i < list.Count; i++)
			{
				outThings.Add(ThingMaker.MakeThing(list[i].thing, list[i].stuff));
			}
			ItemCollectionGeneratorByTotalValueUtility.IncreaseStackCountsToTotalValue(outThings, totalValue, (Thing x) => x.def.ingestible.nutrition);
		}
	}
}
