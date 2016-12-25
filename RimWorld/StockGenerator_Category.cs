using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Category : StockGenerator
	{
		private ThingCategoryDef categoryDef;

		private IntRange thingDefCountRange = IntRange.one;

		private List<ThingDef> excludedThingDefs;

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(Map forMap)
		{
			List<ThingDef> generatedDefs = new List<ThingDef>();
			int numThingDefsToUse = this.thingDefCountRange.RandomInRange;
			for (int i = 0; i < numThingDefsToUse; i++)
			{
				ThingDef chosenThingDef;
				if (!(from t in this.categoryDef.DescendantThingDefs
				where t.tradeability == Tradeability.Stockable && t.techLevel <= this.<>f__this.maxTechLevelGenerate && !this.<generatedDefs>__0.Contains(t) && (this.<>f__this.excludedThingDefs == null || !this.<>f__this.excludedThingDefs.Contains(t))
				select t).TryRandomElement(out chosenThingDef))
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
			return this.categoryDef.DescendantThingDefs.Contains(thingDef) && thingDef.techLevel <= this.maxTechLevelBuy && (this.excludedThingDefs == null || !this.excludedThingDefs.Contains(thingDef));
		}
	}
}
