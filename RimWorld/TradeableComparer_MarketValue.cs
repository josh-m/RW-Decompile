using System;

namespace RimWorld
{
	public class TradeableComparer_MarketValue : TradeableComparer
	{
		public override int Compare(Tradeable lhs, Tradeable rhs)
		{
			return lhs.AnyThing.MarketValue.CompareTo(rhs.AnyThing.MarketValue);
		}
	}
}
