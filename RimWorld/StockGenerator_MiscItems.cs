using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class StockGenerator_MiscItems : StockGenerator
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			int count = this.countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				ThingDef finalDef;
				if (!(from t in DefDatabase<ThingDef>.AllDefs
				where this.<>f__this.HandlesThingDef(t) && t.techLevel <= this.<>f__this.maxTechLevelGenerate
				select t).TryRandomElementByWeight(new Func<ThingDef, float>(this.SelectionWeight), out finalDef))
				{
					break;
				}
				yield return this.MakeThing(finalDef);
			}
		}

		protected virtual Thing MakeThing(ThingDef def)
		{
			return StockGeneratorUtility.TryMakeForStockSingle(def, 1);
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.tradeability == Tradeability.Stockable && thingDef.techLevel <= this.maxTechLevelBuy;
		}

		protected virtual float SelectionWeight(ThingDef thingDef)
		{
			return 1f;
		}
	}
}
