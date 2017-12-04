using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Bed : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			ThingDef singleThingDef = rp.singleThingDef ?? Rand.Element<ThingDef>(ThingDefOf.Bed, ThingDefOf.Bedroll, ThingDefOf.SleepingSpot);
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = singleThingDef;
			bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit = rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
			resolveParams.skipSingleThingIfHasToWipeBuildingOrDoesntFit = new bool?(!skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue || skipSingleThingIfHasToWipeBuildingOrDoesntFit.Value);
			BaseGen.symbolStack.Push("thing", resolveParams);
		}
	}
}
