using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_RandomlyPlaceMealsOnTables : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef singleThingDef = (rp.faction != null && rp.faction.def.techLevel.IsNeolithicOrWorse()) ? ThingDefOf.Pemmican : ThingDefOf.MealSimple;
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].def.IsTable && Rand.Chance(0.15f))
					{
						ResolveParams resolveParams = rp;
						resolveParams.rect = CellRect.SingleCell(iterator.Current);
						resolveParams.singleThingDef = singleThingDef;
						BaseGen.symbolStack.Push("thing", resolveParams);
					}
				}
				iterator.MoveNext();
			}
		}
	}
}
