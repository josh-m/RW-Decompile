using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuyArt : StockGenerator
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.thingClass == typeof(Building_Art);
		}
	}
}
