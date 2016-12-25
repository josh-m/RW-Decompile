using System;

namespace RimWorld
{
	public class TransferableComparer_Quality : TransferableComparer
	{
		public override int Compare(ITransferable lhs, ITransferable rhs)
		{
			return this.GetValueFor(lhs).CompareTo(this.GetValueFor(rhs));
		}

		private int GetValueFor(ITransferable t)
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
