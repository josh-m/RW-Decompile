using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Tag : StockGenerator
	{
		private string tradeTag;

		private IntRange thingDefCountRange = IntRange.one;

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			List<ThingDef> generatedDefs = new List<ThingDef>();
			int numThingDefsToUse = this.thingDefCountRange.RandomInRange;
			for (int i = 0; i < numThingDefsToUse; i++)
			{
				ThingDef chosenThingDef;
				if (!(from d in DefDatabase<ThingDef>.AllDefs
				where this.$this.HandlesThingDef(d) && d.tradeability == Tradeability.Stockable && !generatedDefs.Contains(d)
				select d).TryRandomElement(out chosenThingDef))
				{
					break;
				}
				foreach (Thing th in StockGeneratorUtility.TryMakeForStock(chosenThingDef, base.RandomCountOf(chosenThingDef)))
				{
					yield return th;
				}
				generatedDefs.Add(chosenThingDef);
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.tradeTags != null && thingDef.tradeability != Tradeability.Never && thingDef.techLevel <= this.maxTechLevelBuy && thingDef.tradeTags.Contains(this.tradeTag);
		}
	}
}
