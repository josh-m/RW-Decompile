using System;

namespace RimWorld
{
	public class TradeableComparer_None : TradeableComparer
	{
		public override int Compare(Tradeable lhs, Tradeable rhs)
		{
			return 0;
		}
	}
}
