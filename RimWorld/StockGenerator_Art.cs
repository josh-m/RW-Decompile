using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Art : StockGenerator
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings()
		{
			int count = this.countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				ThingDef artDef;
				if (!(from t in DefDatabase<ThingDef>.AllDefs
				where this.<>f__this.HandlesThingDef(t)
				select t).TryRandomElement(out artDef))
				{
					break;
				}
				Thing art = ThingMaker.MakeThing(artDef, GenStuff.RandomStuffFor(artDef));
				yield return art;
			}
		}

		public override bool HandlesThingDef(ThingDef def)
		{
			return def.tradeability == Tradeability.Stockable && def.Minifiable && def.category == ThingCategory.Building && def.thingClass == typeof(Building_Art) && def.techLevel <= this.maxTechLevelBuy;
		}
	}
}
