using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class HivesUtility
	{
		public static int TotalSpawnedHivesCount(Map map)
		{
			int num = 0;
			List<Thing> allThings = map.listerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				if (allThings[i] is Hive)
				{
					num++;
				}
			}
			return num;
		}
	}
}
