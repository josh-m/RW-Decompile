using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class BillRepeatModeUtility
	{
		public static string GetLabel(this BillRepeatMode brm)
		{
			switch (brm)
			{
			case BillRepeatMode.RepeatCount:
				return "DoXTimes".Translate();
			case BillRepeatMode.TargetCount:
				return "DoUntilYouHaveX".Translate();
			case BillRepeatMode.Forever:
				return "DoForever".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static void MakeConfigFloatMenu(Bill_Production bill)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("DoXTimes".Translate(), delegate
			{
				bill.repeatMode = BillRepeatMode.RepeatCount;
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			FloatMenuOption item = new FloatMenuOption("DoUntilYouHaveX".Translate(), delegate
			{
				if (!bill.recipe.WorkerCounter.CanCountProducts(bill))
				{
					Messages.Message("RecipeCannotHaveTargetCount".Translate(), MessageSound.RejectInput);
				}
				else
				{
					bill.repeatMode = BillRepeatMode.TargetCount;
				}
			}, MenuOptionPriority.Default, null, null, 0f, null, null);
			list.Add(item);
			list.Add(new FloatMenuOption("DoForever".Translate(), delegate
			{
				bill.repeatMode = BillRepeatMode.Forever;
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
