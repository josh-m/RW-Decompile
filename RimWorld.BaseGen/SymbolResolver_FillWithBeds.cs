using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FillWithBeds : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			bool @bool = Rand.Bool;
			ThingDef thingDef = rp.singleThingDef ?? Rand.Element<ThingDef>(ThingDefOf.Bed, ThingDefOf.Bedroll, ThingDefOf.SleepingSpot);
			ThingDef arg_6B_0;
			if ((arg_6B_0 = rp.singleThingStuff) == null)
			{
				arg_6B_0 = GenStuff.RandomStuffByCommonalityFor(thingDef, (rp.faction == null) ? TechLevel.Undefined : rp.faction.def.techLevel);
			}
			ThingDef singleThingStuff = arg_6B_0;
			foreach (IntVec3 current in rp.rect)
			{
				if (@bool)
				{
					if (current.x % 3 != 0 || current.z % 2 != 0)
					{
						continue;
					}
				}
				else if (current.x % 2 != 0 || current.z % 3 != 0)
				{
					continue;
				}
				Rot4 rot = (!@bool) ? Rot4.North : Rot4.West;
				if (!GenSpawn.WouldWipeAnythingWith(current, rot, thingDef, map, (Thing x) => x.def.category == ThingCategory.Building))
				{
					if (!BaseGenUtility.AnyDoorAdjacentCardinalTo(GenAdj.OccupiedRect(current, rot, thingDef.Size), map))
					{
						ResolveParams resolveParams = rp;
						resolveParams.rect = GenAdj.OccupiedRect(current, rot, thingDef.size);
						resolveParams.singleThingDef = thingDef;
						resolveParams.singleThingStuff = singleThingStuff;
						resolveParams.thingRot = new Rot4?(rot);
						BaseGen.symbolStack.Push("bed", resolveParams);
					}
				}
			}
		}
	}
}
