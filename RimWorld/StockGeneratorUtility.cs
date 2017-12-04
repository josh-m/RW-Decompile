using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGeneratorUtility
	{
		[DebuggerHidden]
		public static IEnumerable<Thing> TryMakeForStock(ThingDef thingDef, int count)
		{
			if (thingDef.MadeFromStuff)
			{
				for (int i = 0; i < count; i++)
				{
					Thing th = StockGeneratorUtility.TryMakeForStockSingle(thingDef, 1);
					if (th != null)
					{
						yield return th;
					}
				}
			}
			else
			{
				Thing th2 = StockGeneratorUtility.TryMakeForStockSingle(thingDef, count);
				if (th2 != null)
				{
					yield return th2;
				}
			}
		}

		public static Thing TryMakeForStockSingle(ThingDef thingDef, int stackCount)
		{
			if (stackCount <= 0)
			{
				return null;
			}
			if (thingDef.tradeability != Tradeability.Stockable)
			{
				Log.Error("Tried to make non-Stockable thing for trader stock: " + thingDef);
				return null;
			}
			ThingDef stuff = null;
			if (thingDef.MadeFromStuff)
			{
				stuff = GenStuff.RandomStuffByCommonalityFor(thingDef, TechLevel.Undefined);
			}
			Thing thing = ThingMaker.MakeThing(thingDef, stuff);
			thing.stackCount = stackCount;
			return thing;
		}
	}
}
