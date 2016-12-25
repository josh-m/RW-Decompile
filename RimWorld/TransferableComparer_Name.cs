using System;

namespace RimWorld
{
	public class TransferableComparer_Name : TransferableComparer
	{
		public override int Compare(ITransferable lhs, ITransferable rhs)
		{
			return lhs.Label.CompareTo(rhs.Label);
		}
	}
}
