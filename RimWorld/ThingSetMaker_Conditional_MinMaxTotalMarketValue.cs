using System;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Conditional_MinMaxTotalMarketValue : ThingSetMaker_Conditional
	{
		public float minMaxTotalMarketValue;

		protected override bool Condition(ThingSetMakerParams parms)
		{
			FloatRange? totalMarketValueRange = parms.totalMarketValueRange;
			return totalMarketValueRange.HasValue && parms.totalMarketValueRange.Value.max >= this.minMaxTotalMarketValue;
		}
	}
}
