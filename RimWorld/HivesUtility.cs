using System;
using Verse;

namespace RimWorld
{
	public static class HivesUtility
	{
		public static int TotalSpawnedHivesCount(Map map)
		{
			return map.listerThings.ThingsOfDef(ThingDefOf.Hive).Count;
		}
	}
}
