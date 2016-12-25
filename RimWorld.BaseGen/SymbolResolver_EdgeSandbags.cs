using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EdgeSandbags : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			float? chanceToSkipSandbag = rp.chanceToSkipSandbag;
			float chance = (!chanceToSkipSandbag.HasValue) ? 0f : chanceToSkipSandbag.Value;
			foreach (IntVec3 current in rp.rect.EdgeCells)
			{
				Rand.PushSeed();
				Rand.Seed = Gen.HashCombineInt(current.x / 2, current.z / 2);
				bool flag = Rand.Chance(chance);
				Rand.PopSeed();
				if (!flag)
				{
					if (current.Standable(map) && !current.Roofed(map) && !current.Fogged(map))
					{
						if (!GenSpawn.WouldWipeAnythingWith(current, Rot4.North, ThingDefOf.Sandbags, map, (Thing x) => x.def.category == ThingCategory.Building || x.def.category == ThingCategory.Item))
						{
							Thing thing = ThingMaker.MakeThing(ThingDefOf.Sandbags, null);
							thing.SetFaction(rp.faction, null);
							GenSpawn.Spawn(thing, current, map);
						}
					}
				}
			}
		}
	}
}
