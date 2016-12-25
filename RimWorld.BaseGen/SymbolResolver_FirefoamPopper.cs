using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FirefoamPopper : SymbolResolver
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
			Thing thing = ThingMaker.MakeThing(ThingDefOf.FirefoamPopper, null);
			thing.SetFaction(rp.faction, null);
			GenSpawn.Spawn(thing, loc, BaseGen.globalSettings.map);
		}

		private bool TryFindSpawnCell(CellRect rect, out IntVec3 result)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 c)
			{
				bool arg_6C_0;
				if (c.Standable(map) && !BaseGenUtility.AnyDoorAdjacentTo(c, map) && c.GetFirstItem(map) == null)
				{
					arg_6C_0 = !GenSpawn.WouldWipeAnythingWith(c, Rot4.North, ThingDefOf.FirefoamPopper, map, (Thing x) => x.def.category == ThingCategory.Building);
				}
				else
				{
					arg_6C_0 = false;
				}
				return arg_6C_0;
			}, out result);
		}
	}
}
