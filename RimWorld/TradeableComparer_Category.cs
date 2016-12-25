using System;
using Verse;

namespace RimWorld
{
	public class TradeableComparer_Category : TradeableComparer
	{
		public override int Compare(Tradeable lhs, Tradeable rhs)
		{
			ThingDef thingDef = lhs.ThingDef;
			ThingDef thingDef2 = rhs.ThingDef;
			if (thingDef.category != thingDef2.category)
			{
				return thingDef.category.CompareTo(thingDef2.category);
			}
			float listOrderPriority = lhs.ListOrderPriority;
			float listOrderPriority2 = rhs.ListOrderPriority;
			if (listOrderPriority != listOrderPriority2)
			{
				return listOrderPriority.CompareTo(listOrderPriority2);
			}
			int num = 0;
			if (!lhs.AnyThing.def.thingCategories.NullOrEmpty<ThingCategoryDef>())
			{
				num = (int)lhs.AnyThing.def.thingCategories[0].index;
			}
			int value = 0;
			if (!rhs.AnyThing.def.thingCategories.NullOrEmpty<ThingCategoryDef>())
			{
				value = (int)rhs.AnyThing.def.thingCategories[0].index;
			}
			return num.CompareTo(value);
		}
	}
}
