using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class BillRepeatModeUtility
	{
		public static string GetLabel(this BillRepeatModeDef brm)
		{
			if (brm == BillRepeatModeDefOf.RepeatCount)
			{
				return "DoXTimes".Translate();
			}
			if (brm == BillRepeatModeDefOf.TargetCount)
			{
				return "DoUntilYouHaveX".Translate();
			}
			if (brm == BillRepeatModeDefOf.Forever)
			{
				return "DoForever".Translate();
			}
			throw new ArgumentException();
		}

		public static void MakeConfigFloatMenu(Bill_Production bill)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("DoXTimes".Translate(), delegate
			{
				bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			FloatMenuOption item = new FloatMenuOption("DoUntilYouHaveX".Translate(), delegate
			{
				if (!bill.recipe.WorkerCounter.CanCountProducts(bill))
				{
					Messages.Message("RecipeCannotHaveTargetCount".Translate(), MessageSound.RejectInput);
				}
				else
				{
					bill.repeatMode = BillRepeatModeDefOf.TargetCount;
				}
			}, MenuOptionPriority.Default, null, null, 0f, null, null);
			list.Add(item);
			list.Add(new FloatMenuOption("DoForever".Translate(), delegate
			{
				bill.repeatMode = BillRepeatModeDefOf.Forever;
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
