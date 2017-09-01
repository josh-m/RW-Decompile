using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Refuel : SymbolResolver
	{
		private static List<CompRefuelable> refuelables = new List<CompRefuelable>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_Refuel.refuelables.Clear();
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					CompRefuelable compRefuelable = thingList[i].TryGetComp<CompRefuelable>();
					if (compRefuelable != null && !SymbolResolver_Refuel.refuelables.Contains(compRefuelable))
					{
						SymbolResolver_Refuel.refuelables.Add(compRefuelable);
					}
				}
				iterator.MoveNext();
			}
			for (int j = 0; j < SymbolResolver_Refuel.refuelables.Count; j++)
			{
				float fuelCapacity = SymbolResolver_Refuel.refuelables[j].Props.fuelCapacity;
				float amount = Rand.Range(fuelCapacity / 2f, fuelCapacity);
				SymbolResolver_Refuel.refuelables[j].Refuel(amount);
			}
			SymbolResolver_Refuel.refuelables.Clear();
		}
	}
}
