using System;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Art : StockGenerator_MiscItems
	{
		public override bool HandlesThingDef(ThingDef td)
		{
			return base.HandlesThingDef(td) && td.Minifiable && td.category == ThingCategory.Building && td.thingClass == typeof(Building_Art);
		}
	}
}
