using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_SingleThing : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			IntVec3 intVec;
			return base.CanResolve(rp) && (rp.singleThingDef == null || rp.singleThingDef.category != ThingCategory.Item || this.TryFindClearCellForItem(rp.rect, out intVec));
		}

		public override void Resolve(ResolveParams rp)
		{
			ThingDef arg_3A_0;
			if ((arg_3A_0 = rp.singleThingDef) == null)
			{
				arg_3A_0 = (from x in DefDatabase<ThingDef>.AllDefsListForReading
				where (x.IsWeapon || x.IsMedicine || x.IsDrug) && !x.MadeFromStuff && x.graphicData != null
				select x).RandomElement<ThingDef>();
			}
			ThingDef thingDef = arg_3A_0;
			Rot4? thingRot = rp.thingRot;
			Rot4 rot = (!thingRot.HasValue) ? Rot4.North : thingRot.Value;
			IntVec3 intVec;
			if (thingDef.category == ThingCategory.Item)
			{
				if (!this.TryFindClearCellForItem(rp.rect, out intVec))
				{
					return;
				}
			}
			else
			{
				intVec = rp.rect.RandomCell;
				Map map = BaseGen.globalSettings.map;
				if (!intVec.Standable(map))
				{
					foreach (IntVec3 current in rp.rect.Cells.InRandomOrder(null))
					{
						if (current.Standable(map))
						{
							intVec = current;
							break;
						}
					}
				}
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, null), intVec, BaseGen.globalSettings.map, rot);
			Thing arg_144_0 = thing;
			int? singleThingStackCount = rp.singleThingStackCount;
			arg_144_0.stackCount = ((!singleThingStackCount.HasValue) ? 1 : singleThingStackCount.Value);
			if (thing.def.CanHaveFaction && thing.Faction != rp.faction)
			{
				thing.SetFaction(rp.faction, null);
			}
		}

		private bool TryFindClearCellForItem(CellRect rect, out IntVec3 result)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					return false;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					ThingDef def = thingList[i].def;
					if (def.category == ThingCategory.Item || def.category == ThingCategory.Plant)
					{
						return false;
					}
				}
				return true;
			}, out result);
		}
	}
}
