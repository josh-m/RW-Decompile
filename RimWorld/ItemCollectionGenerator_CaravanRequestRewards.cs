using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_CaravanRequestRewards : ItemCollectionGenerator_Rewards
	{
		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int? count = parms.count;
			parms.count = new int?((!count.HasValue) ? 1 : count.Value);
			float? totalMarketValue = parms.totalMarketValue;
			float arg_98_0;
			if (totalMarketValue.HasValue)
			{
				arg_98_0 = totalMarketValue.Value;
			}
			else
			{
				FloatRange floatRange = new FloatRange((float)IncidentWorker_CaravanRequest.BaseValueWantedRange.min * IncidentWorker_CaravanRequest.RewardMarketValueFactorRange.min, (float)IncidentWorker_CaravanRequest.BaseValueWantedRange.max * IncidentWorker_CaravanRequest.RewardMarketValueFactorRange.max);
				arg_98_0 = floatRange.RandomInRange;
			}
			parms.totalMarketValue = new float?(arg_98_0);
			base.Generate(parms, outThings);
		}
	}
}
