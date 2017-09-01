using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuySingleDef : StockGenerator
	{
		public ThingDef thingDef;

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef == this.thingDef;
		}
	}
}
