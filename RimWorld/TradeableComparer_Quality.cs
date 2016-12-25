using System;

namespace RimWorld
{
	public class TradeableComparer_Quality : TradeableComparer
	{
		public override int Compare(Tradeable lhs, Tradeable rhs)
		{
			return this.GetValueFor(lhs).CompareTo(this.GetValueFor(rhs));
		}

		private int GetValueFor(Tradeable t)
		{
			QualityCategory result;
			if (!t.AnyThing.TryGetQuality(out result))
			{
				return -1;
			}
			return (int)result;
		}
	}
}
