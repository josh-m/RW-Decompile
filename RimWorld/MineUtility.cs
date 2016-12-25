using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class MineUtility
	{
		public static Thing MineableInCell(IntVec3 loc)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(loc);
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
