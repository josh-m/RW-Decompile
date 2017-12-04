using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Animals : GenStep
	{
		public override void Generate(Map map)
		{
			int num = 0;
			while (!map.wildSpawner.AnimalEcosystemFull)
			{
				num++;
				if (num >= 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				IntVec3 loc = RCellFinder.RandomAnimalSpawnCell_MapGen(map);
				if (!map.wildSpawner.SpawnRandomWildAnimalAt(loc))
				{
					break;
				}
			}
		}
	}
}
