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
		public override IEnumerable<Thing> GenerateThings(int forTile)
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

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
		{
			foreach (string e in base.ConfigErrors(parentDef))
			{
				yield return e;
			}
			for (int i = 0; i < this.thingDefs.Count; i++)
			{
				if (this.thingDefs[i].tradeability != Tradeability.Stockable)
				{
					yield return this.thingDefs[i] + " is not Stockable";
				}
			}
		}
	}
}
