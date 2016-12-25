using System;
using Verse;

namespace RimWorld
{
	public class StockGenerator_WeaponsRanged : StockGenerator_MiscItems
	{
		public override bool HandlesThingDef(ThingDef td)
		{
			return base.HandlesThingDef(td) && td.IsRangedWeapon;
		}
	}
}
