using System;
using Verse;

namespace RimWorld
{
	public class TransferableComparer_Category : TransferableComparer
	{
		public override int Compare(ITransferable lhs, ITransferable rhs)
		{
			ThingDef thingDef = lhs.ThingDef;
			ThingDef thingDef2 = rhs.ThingDef;
			if (thingDef.category != thingDef2.category)
			{
				return thingDef.category.CompareTo(thingDef2.category);
			}
			float num = TransferableUIUtility.DefaultListOrderPriority(lhs);
			float num2 = TransferableUIUtility.DefaultListOrderPriority(rhs);
			if (num != num2)
			{
				return num.CompareTo(num2);
			}
			int num3 = 0;
			if (!lhs.AnyThing.def.thingCategories.NullOrEmpty<ThingCategoryDef>())
			{
				num3 = (int)lhs.AnyThing.def.thingCategories[0].index;
			}
			int value = 0;
			if (!rhs.AnyThing.def.thingCategories.NullOrEmpty<ThingCategoryDef>())
			{
				value = (int)rhs.AnyThing.def.thingCategories[0].index;
			}
			return num3.CompareTo(value);
		}
	}
}
