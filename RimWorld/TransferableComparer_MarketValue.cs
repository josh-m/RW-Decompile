using System;

namespace RimWorld
{
	public class TransferableComparer_MarketValue : TransferableComparer
	{
		public override int Compare(ITransferable lhs, ITransferable rhs)
		{
			return lhs.AnyThing.MarketValue.CompareTo(rhs.AnyThing.MarketValue);
		}
	}
}
