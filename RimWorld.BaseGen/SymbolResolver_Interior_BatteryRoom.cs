using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_BatteryRoom : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("indoorLighting", rp);
			BaseGen.symbolStack.Push("chargeBatteries", rp);
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = ThingDefOf.Battery;
			resolveParams.thingRot = new Rot4?((!Rand.Bool) ? Rot4.East : Rot4.North);
			int? fillWithThingsPadding = rp.fillWithThingsPadding;
			resolveParams.fillWithThingsPadding = new int?((!fillWithThingsPadding.HasValue) ? 1 : fillWithThingsPadding.Value);
			BaseGen.symbolStack.Push("fillWithThings", resolveParams);
		}
	}
}
