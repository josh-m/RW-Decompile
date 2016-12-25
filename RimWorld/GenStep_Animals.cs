using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Animals : GenStep
	{
		public override void Generate(Map map)
		{
			while (!map.wildSpawner.AnimalEcosystemFull)
			{
				IntVec3 loc = RCellFinder.RandomAnimalSpawnCell_MapGen(map);
				map.wildSpawner.SpawnRandomWildAnimalAt(loc);
			}
		}
	}
}
