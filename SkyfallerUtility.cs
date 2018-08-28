using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

public static class SkyfallerUtility
{
	public static bool CanPossiblyFallOnColonist(ThingDef skyfaller, IntVec3 c, Map map)
	{
		CellRect cellRect = GenAdj.OccupiedRect(c, Rot4.North, skyfaller.size);
		int dist = Mathf.Max(Mathf.CeilToInt(skyfaller.skyfaller.explosionRadius) + 7, 14);
		CellRect.CellRectIterator iterator = cellRect.ExpandedBy(dist).GetIterator();
		while (!iterator.Done())
		{
			IntVec3 current = iterator.Current;
			if (current.InBounds(map))
			{
				List<Thing> thingList = current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && pawn.IsColonist)
					{
						return true;
					}
				}
			}
			iterator.MoveNext();
		}
		return false;
	}
}
