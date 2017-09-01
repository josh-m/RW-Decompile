using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_PlaceChairsNearTables : SymbolResolver
	{
		private static List<Thing> tables = new List<Thing>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_PlaceChairsNearTables.tables.Clear();
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].def.IsTable && !SymbolResolver_PlaceChairsNearTables.tables.Contains(thingList[i]))
					{
						SymbolResolver_PlaceChairsNearTables.tables.Add(thingList[i]);
					}
				}
				iterator.MoveNext();
			}
			for (int j = 0; j < SymbolResolver_PlaceChairsNearTables.tables.Count; j++)
			{
				CellRect cellRect = SymbolResolver_PlaceChairsNearTables.tables[j].OccupiedRect().ExpandedBy(1);
				bool flag = false;
				foreach (IntVec3 current in cellRect.EdgeCells.InRandomOrder(null))
				{
					if (!cellRect.IsCorner(current) && rp.rect.Contains(current))
					{
						if (current.Standable(map) && current.GetEdifice(map) == null)
						{
							if (!flag || !Rand.Bool)
							{
								Rot4 value;
								if (current.x == cellRect.minX)
								{
									value = Rot4.East;
								}
								else if (current.x == cellRect.maxX)
								{
									value = Rot4.West;
								}
								else if (current.z == cellRect.minZ)
								{
									value = Rot4.North;
								}
								else
								{
									value = Rot4.South;
								}
								ResolveParams resolveParams = rp;
								resolveParams.rect = CellRect.SingleCell(current);
								resolveParams.singleThingDef = ThingDefOf.DiningChair;
								resolveParams.singleThingStuff = (rp.singleThingStuff ?? ThingDefOf.WoodLog);
								resolveParams.thingRot = new Rot4?(value);
								BaseGen.symbolStack.Push("thing", resolveParams);
								flag = true;
							}
						}
					}
				}
			}
			SymbolResolver_PlaceChairsNearTables.tables.Clear();
		}
	}
}
