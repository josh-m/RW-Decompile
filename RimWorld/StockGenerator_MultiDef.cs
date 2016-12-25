using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGenerator_MultiDef : StockGenerator
	{
		private List<ThingDef> thingDefs = new List<ThingDef>();

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings()
		{
			ThingDef td = this.thingDefs.RandomElement<ThingDef>();
			foreach (Thing th in StockGeneratorUtility.TryMakeForStock(td, base.RandomCountOf(td)))
			{
				yield return th;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return this.thingDefs.Contains(thingDef);
		}
	}
}
