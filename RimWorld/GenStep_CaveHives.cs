using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class GenStep_CaveHives : GenStep
	{
		private List<IntVec3> rockCells = new List<IntVec3>();

		private List<IntVec3> possibleSpawnCells = new List<IntVec3>();

		private List<Hive> spawnedHives = new List<Hive>();

		private const int MinDistToOpenSpace = 10;

		private const float CaveCellsPerHive = 1000f;

		public override void Generate(Map map)
		{
			if (!Find.Storyteller.difficulty.allowCaveHives)
			{
				return;
			}
			CompProperties_TemperatureDamaged compProperties = ThingDefOf.Hive.GetCompProperties<CompProperties_TemperatureDamaged>();
			if (!compProperties.safeTemperatureRange.Includes(map.mapTemperature.OutdoorTemp))
			{
				return;
			}
			MapGenFloatGrid caves = MapGenerator.Caves;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			float num = 0.7f;
			int num2 = 0;
			this.rockCells.Clear();
			foreach (IntVec3 current in map.AllCells)
			{
				if (elevation[current] > num)
				{
					this.rockCells.Add(current);
				}
				if (caves[current] > 0f)
				{
					num2++;
				}
			}
			int num3 = GenMath.RoundRandom((float)num2 / 1000f);
			GenMorphology.Erode(this.rockCells, 10, map, null);
			this.possibleSpawnCells.Clear();
			for (int i = 0; i < this.rockCells.Count; i++)
			{
				if (caves[this.rockCells[i]] > 0f)
				{
					this.possibleSpawnCells.Add(this.rockCells[i]);
				}
			}
			this.spawnedHives.Clear();
			for (int j = 0; j < num3; j++)
			{
				this.TrySpawnHive(map);
			}
			this.spawnedHives.Clear();
		}

		private void TrySpawnHive(Map map)
		{
			IntVec3 intVec;
			if (!this.TryFindHiveSpawnCell(map, out intVec))
			{
				return;
			}
			this.possibleSpawnCells.Remove(intVec);
			Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive, null), intVec, map);
			hive.SetFaction(Faction.OfInsects, null);
			(from x in hive.GetComps<CompSpawner>()
			where x.PropsSpawner.thingToSpawn == ThingDefOf.GlowPod
			select x).First<CompSpawner>().TryDoSpawn();
			hive.SpawnPawnsUntilPoints(Rand.Range(200f, 500f));
			hive.canSpawnPawns = false;
			hive.GetComp<CompSpawnerHives>().canSpawnHives = false;
			this.spawnedHives.Add(hive);
		}

		private bool TryFindHiveSpawnCell(Map map, out IntVec3 spawnCell)
		{
			float num = -1f;
			IntVec3 intVec = IntVec3.Invalid;
			for (int i = 0; i < 3; i++)
			{
				IntVec3 intVec2;
				if (!(from x in this.possibleSpawnCells
				where x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null && x.GetFirstPawn(map) == null
				select x).TryRandomElement(out intVec2))
				{
					break;
				}
				float num2 = -1f;
				for (int j = 0; j < this.spawnedHives.Count; j++)
				{
					float num3 = (float)intVec2.DistanceToSquared(this.spawnedHives[j].Position);
					if (num2 < 0f || num3 < num2)
					{
						num2 = num3;
					}
				}
				if (!intVec.IsValid || num2 > num)
				{
					intVec = intVec2;
					num = num2;
				}
			}
			spawnCell = intVec;
			return spawnCell.IsValid;
		}
	}
}
