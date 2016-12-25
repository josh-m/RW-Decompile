using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuyWeirdOrganic : StockGenerator
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings()
		{
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef == ThingDefOf.InsectJelly;
		}
	}
}
