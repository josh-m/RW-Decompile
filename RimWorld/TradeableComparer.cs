using System;
using System.Collections.Generic;

namespace RimWorld
{
	public abstract class TradeableComparer : IComparer<Tradeable>
	{
		public abstract int Compare(Tradeable lhs, Tradeable rhs);
	}
}
