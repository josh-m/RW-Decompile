using System;

namespace RimWorld
{
	public class TransferableComparer_None : TransferableComparer
	{
		public override int Compare(ITransferable lhs, ITransferable rhs)
		{
			return 0;
		}
	}
}
