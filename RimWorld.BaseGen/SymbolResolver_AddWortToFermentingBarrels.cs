using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AddWortToFermentingBarrels : SymbolResolver
	{
		private static List<Building_FermentingBarrel> barrels = new List<Building_FermentingBarrel>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_AddWortToFermentingBarrels.barrels.Clear();
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Building_FermentingBarrel building_FermentingBarrel = thingList[i] as Building_FermentingBarrel;
					if (building_FermentingBarrel != null && !SymbolResolver_AddWortToFermentingBarrels.barrels.Contains(building_FermentingBarrel))
					{
						SymbolResolver_AddWortToFermentingBarrels.barrels.Add(building_FermentingBarrel);
					}
				}
				iterator.MoveNext();
			}
			float progress = Rand.Range(0.1f, 0.9f);
			for (int j = 0; j < SymbolResolver_AddWortToFermentingBarrels.barrels.Count; j++)
			{
				if (!SymbolResolver_AddWortToFermentingBarrels.barrels[j].Fermented)
				{
					int num = Rand.RangeInclusive(1, 25);
					num = Mathf.Min(num, SymbolResolver_AddWortToFermentingBarrels.barrels[j].SpaceLeftForWort);
					if (num > 0)
					{
						SymbolResolver_AddWortToFermentingBarrels.barrels[j].AddWort(num);
						SymbolResolver_AddWortToFermentingBarrels.barrels[j].Progress = progress;
					}
				}
			}
			SymbolResolver_AddWortToFermentingBarrels.barrels.Clear();
		}
	}
}
