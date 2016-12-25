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
			ThingDef thingDef = rp.singleThingDef ?? ((!Rand.Bool) ? ThingDefOf.SleepingSpot : ThingDefOf.Bed);
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
					bool flag = false;
					foreach (IntVec3 current2 in GenAdj.CellsOccupiedBy(current, rot, thingDef.Size))
					{
						if (BaseGenUtility.AnyDoorAdjacentTo(current2, map))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						ThingDef stuff = null;
						if (thingDef.MadeFromStuff)
						{
							stuff = ThingDefOf.WoodLog;
						}
						Thing thing = ThingMaker.MakeThing(thingDef, stuff);
						thing.SetFaction(rp.faction, null);
						GenSpawn.Spawn(thing, current, map, rot);
					}
				}
			}
		}
	}
}
