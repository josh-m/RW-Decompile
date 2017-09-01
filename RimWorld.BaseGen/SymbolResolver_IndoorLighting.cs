using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_IndoorLighting : SymbolResolver
	{
		private const float NeverSpawnTorchesIfTemperatureAbove = 18f;

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef;
			if (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Industrial)
			{
				thingDef = ThingDefOf.StandingLamp;
			}
			else if (map.mapTemperature.OutdoorTemp > 18f)
			{
				thingDef = null;
			}
			else
			{
				thingDef = ThingDefOf.TorchLamp;
			}
			if (thingDef != null)
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingDef = thingDef;
				BaseGen.symbolStack.Push("edgeThing", resolveParams);
			}
		}
	}
}
