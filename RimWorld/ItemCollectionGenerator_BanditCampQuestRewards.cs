using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_BanditCampQuestRewards : ItemCollectionGenerator_Rewards
	{
		private static readonly IntRange RewardMarketValueRange = new IntRange(2000, 3000);

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int? count = parms.count;
			parms.count = new int?((!count.HasValue) ? 1 : count.Value);
			float? totalMarketValue = parms.totalMarketValue;
			parms.totalMarketValue = new float?((!totalMarketValue.HasValue) ? ((float)ItemCollectionGenerator_BanditCampQuestRewards.RewardMarketValueRange.RandomInRange) : totalMarketValue.Value);
			base.Generate(parms, outThings);
		}
	}
}
