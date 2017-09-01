using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGenerator_SingleDef : StockGenerator
	{
		private ThingDef thingDef;

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			foreach (Thing th in StockGeneratorUtility.TryMakeForStock(this.thingDef, base.RandomCountOf(this.thingDef)))
			{
				yield return th;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef == this.thingDef;
		}
	}
}
