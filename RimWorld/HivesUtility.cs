using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class HivesUtility
	{
		public static int TotalSpawnedHivesCount
		{
			get
			{
				int num = 0;
				List<Thing> allThings = Find.ListerThings.AllThings;
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
}
