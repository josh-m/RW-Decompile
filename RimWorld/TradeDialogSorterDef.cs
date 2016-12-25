using System;
using Verse;

namespace RimWorld
{
	public class TradeDialogSorterDef : Def
	{
		public Type comparerClass;

		[Unsaved]
		private TradeableComparer comparerInt;

		public TradeableComparer Comparer
		{
			get
			{
				if (this.comparerInt == null)
				{
					this.comparerInt = (TradeableComparer)Activator.CreateInstance(this.comparerClass);
				}
				return this.comparerInt;
			}
		}
	}
}
