using System;
using System.Collections.Generic;

namespace RimWorld
{
	public abstract class TransferableComparer : IComparer<ITransferable>
	{
		public abstract int Compare(ITransferable lhs, ITransferable rhs);
	}
}
