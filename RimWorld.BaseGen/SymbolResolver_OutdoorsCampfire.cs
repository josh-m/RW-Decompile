using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_OutdoorsCampfire : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			IntVec3 intVec;
			return base.CanResolve(rp) && this.TryFindSpawnCell(rp.rect, out intVec);
		}

		public override void Resolve(ResolveParams rp)
		{
			IntVec3 loc;
			if (!this.TryFindSpawnCell(rp.rect, out loc))
			{
				return;
			}
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Campfire, null);
			thing.SetFaction(rp.faction, null);
			GenSpawn.Spawn(thing, loc, BaseGen.globalSettings.map);
		}

		private bool TryFindSpawnCell(CellRect rect, out IntVec3 result)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 c)
			{
				bool arg_7D_0;
				if (c.Standable(map) && !c.Roofed(map) && !BaseGenUtility.AnyDoorAdjacentTo(c, map) && c.GetFirstItem(map) == null)
				{
					arg_7D_0 = !GenSpawn.WouldWipeAnythingWith(c, Rot4.North, ThingDefOf.Campfire, map, (Thing x) => x.def.category == ThingCategory.Building);
				}
				else
				{
					arg_7D_0 = false;
				}
				return arg_7D_0;
			}, out result);
		}
	}
}
