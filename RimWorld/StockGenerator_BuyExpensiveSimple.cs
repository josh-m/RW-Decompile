using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuyExpensiveSimple : StockGenerator
	{
		public float minValuePerUnit = 15f;

		[DebuggerHidden]
		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Item && !thingDef.IsApparel && !thingDef.IsWeapon && !thingDef.IsMedicine && !thingDef.IsDrug && thingDef.BaseMarketValue / thingDef.VolumePerUnit > this.minValuePerUnit;
		}
	}
}
