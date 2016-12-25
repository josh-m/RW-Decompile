using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class MineUtility
	{
		public static Thing MineableInCell(IntVec3 loc, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(loc);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.mineable)
				{
					return list[i];
				}
			}
			return null;
		}
	}
}
