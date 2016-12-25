using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerHives : CompProperties
	{
		public float HiveSpawnPreferredMinDist = 3.5f;

		public float HiveSpawnRadius = 10f;

		public FloatRange HiveSpawnIntervalDays = new FloatRange(1.6f, 2.1f);

		public CompProperties_SpawnerHives()
		{
			this.compClass = typeof(CompSpawnerHives);
		}
	}
}
