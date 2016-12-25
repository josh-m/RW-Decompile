using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_WeaponsRanged : StockGenerator
	{
		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings()
		{
			int count = this.countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				ThingDef gunDef;
				if (!(from t in DefDatabase<ThingDef>.AllDefs
				where this.<>f__this.HandlesThingDef(t)
				select t).TryRandomElement(out gunDef))
				{
					break;
				}
				ThingWithComps gun = (ThingWithComps)ThingMaker.MakeThing(gunDef, null);
				yield return gun;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.IsRangedWeapon && thingDef.tradeability == Tradeability.Stockable && thingDef.techLevel <= this.maxTechLevelBuy;
		}
	}
}
