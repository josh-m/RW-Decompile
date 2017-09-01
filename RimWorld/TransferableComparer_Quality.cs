using System;

namespace RimWorld
{
	public class TransferableComparer_Quality : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return this.GetValueFor(lhs).CompareTo(this.GetValueFor(rhs));
		}

		private int GetValueFor(Transferable t)
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
