using System;
using Verse;

namespace RimWorld
{
	public class TransferableSorterDef : Def
	{
		public Type comparerClass;

		[Unsaved]
		private TransferableComparer comparerInt;

		public TransferableComparer Comparer
		{
			get
			{
				if (this.comparerInt == null)
				{
					this.comparerInt = (TransferableComparer)Activator.CreateInstance(this.comparerClass);
				}
				return this.comparerInt;
			}
		}
	}
}
