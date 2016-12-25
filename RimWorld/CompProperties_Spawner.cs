using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Spawner : CompProperties
	{
		public ThingDef thingToSpawn;

		public int spawnCount = 1;

		public IntRange spawnIntervalRange = new IntRange(100, 100);

		public int spawnMaxAdjacent = -1;

		public bool spawnForbidden;

		public CompProperties_Spawner()
		{
			this.compClass = typeof(CompSpawner);
		}
	}
}
