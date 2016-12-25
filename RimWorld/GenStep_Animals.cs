using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Animals : GenStep
	{
		public override void Generate()
		{
			while (!WildSpawner.AnimalEcosystemFull)
			{
				IntVec3 loc = RCellFinder.RandomAnimalSpawnCell_MapGen();
				WildSpawner.SpawnRandomWildAnimalAt(loc);
			}
		}
	}
}
