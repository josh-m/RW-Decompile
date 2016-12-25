using System;

namespace RimWorld
{
	public class TradeableComparer_Name : TradeableComparer
	{
		public override int Compare(Tradeable lhs, Tradeable rhs)
		{
			return lhs.Label.CompareTo(rhs.Label);
		}
	}
}
