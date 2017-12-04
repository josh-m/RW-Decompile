using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WildSpawner
	{
		private Map map;

		private static List<ThingDef> cavePlants;

		private const int AnimalCheckInterval = 1210;

		private const float BaseAnimalSpawnChancePerInterval = 0.0268888883f;

		private const int PlantTrySpawnIntervalAt100EdgeLength = 650;

		private const int CavePlantSpawnIntervalPer10kCells = 3600000;

		private static List<IntVec3> undergroundCells = new List<IntVec3>();

		private float DesiredAnimalDensity
		{
			get
			{
				float num = this.map.Biome.animalDensity;
				float num2 = 0f;
				float num3 = 0f;
				foreach (PawnKindDef current in this.map.Biome.AllWildAnimals)
				{
					num3 += current.wildSpawn_EcoSystemWeight;
					if (this.map.mapTemperature.SeasonAcceptableFor(current.race))
					{
						num2 += current.wildSpawn_EcoSystemWeight;
					}
				}
				num *= num2 / num3;
				num *= this.map.gameConditionManager.AggregateAnimalDensityFactor();
				return num;
			}
		}

		private float DesiredTotalAnimalWeight
		{
			get
			{
				float desiredAnimalDensity = this.DesiredAnimalDensity;
				if (desiredAnimalDensity == 0f)
				{
					return 0f;
				}
				float num = 10000f / desiredAnimalDensity;
				return (float)this.map.Area / num;
			}
		}

		private float CurrentTotalAnimalWeight
		{
			get
			{
				float num = 0f;
				List<Pawn> allPawnsSpawned = this.map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].kindDef.wildSpawn_spawnWild && allPawnsSpawned[i].Faction == null)
					{
						num += allPawnsSpawned[i].kindDef.wildSpawn_EcoSystemWeight;
					}
				}
				return num;
			}
		}

		public bool AnimalEcosystemFull
		{
			get
			{
				return this.CurrentTotalAnimalWeight >= this.DesiredTotalAnimalWeight;
			}
		}

		public WildSpawner(Map map)
		{
			this.map = map;
		}

		public static void Reset()
		{
			WildSpawner.cavePlants = (from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.category == ThingCategory.Plant && x.plant.cavePlant
			select x).ToList<ThingDef>();
		}

		public void WildSpawnerTick()
		{
			IntVec3 loc;
			if (Find.TickManager.TicksGame % 1210 == 0 && !this.AnimalEcosystemFull && Rand.Value < 0.0268888883f * this.DesiredAnimalDensity && RCellFinder.TryFindRandomPawnEntryCell(out loc, this.map, CellFinder.EdgeRoadChance_Animal, null))
			{
				this.SpawnRandomWildAnimalAt(loc);
			}
			float num = this.map.gameConditionManager.AggregatePlantDensityFactor();
			if (num > 0.0001f)
			{
				int num2 = this.map.Size.x * 2 + this.map.Size.z * 2;
				float num3 = 650f / ((float)num2 / 100f);
				int num4 = (int)(num3 / num);
				if (num4 <= 0 || Find.TickManager.TicksGame % num4 == 0)
				{
					this.TrySpawnPlantFromMapEdge();
				}
			}
			int num5 = (int)(3600000f / ((float)this.map.Area / 10000f));
			if (num5 <= 0 || Find.TickManager.TicksGame % num5 == 0)
			{
				this.TrySpawnCavePlant();
			}
		}

		private void TrySpawnPlantFromMapEdge()
		{
			ThingDef plantDef;
			if (!this.map.Biome.AllWildPlants.TryRandomElementByWeight((ThingDef def) => this.map.Biome.CommonalityOfPlant(def), out plantDef))
			{
				return;
			}
			IntVec3 source;
			if (RCellFinder.TryFindRandomCellToPlantInFromOffMap(plantDef, this.map, out source))
			{
				GenPlantReproduction.TryReproduceFrom(source, plantDef, SeedTargFindMode.MapEdge, this.map);
			}
		}

		private void TrySpawnCavePlant()
		{
			WildSpawner.undergroundCells.Clear();
			CellRect.CellRectIterator iterator = CellRect.WholeMap(this.map).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (GenPlantReproduction.GoodRoofForCavePlantReproduction(current, this.map))
				{
					if (current.GetFirstItem(this.map) == null && current.GetFirstPawn(this.map) == null && current.GetFirstBuilding(this.map) == null)
					{
						bool flag = false;
						for (int i = 0; i < WildSpawner.cavePlants.Count; i++)
						{
							if (WildSpawner.cavePlants[i].CanEverPlantAt(current, this.map))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							WildSpawner.undergroundCells.Add(current);
						}
					}
				}
				iterator.MoveNext();
			}
			if (WildSpawner.undergroundCells.Any<IntVec3>())
			{
				IntVec3 cell = WildSpawner.undergroundCells.RandomElement<IntVec3>();
				ThingDef plantDef = (from x in WildSpawner.cavePlants
				where x.CanEverPlantAt(cell, this.map)
				select x).RandomElement<ThingDef>();
				GenPlantReproduction.TryReproduceFrom(cell, plantDef, SeedTargFindMode.Cave, this.map);
			}
		}

		public bool SpawnRandomWildAnimalAt(IntVec3 loc)
		{
			PawnKindDef pawnKindDef = (from a in this.map.Biome.AllWildAnimals
			where this.map.mapTemperature.SeasonAcceptableFor(a.race)
			select a).RandomElementByWeight((PawnKindDef def) => this.map.Biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
			if (pawnKindDef == null)
			{
				Log.Error("No spawnable animals right now.");
				return false;
			}
			int randomInRange = pawnKindDef.wildSpawn_GroupSizeRange.RandomInRange;
			int radius = Mathf.CeilToInt(Mathf.Sqrt((float)pawnKindDef.wildSpawn_GroupSizeRange.max));
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, this.map, radius, null);
				Pawn newThing = PawnGenerator.GeneratePawn(pawnKindDef, null);
				GenSpawn.Spawn(newThing, loc2, this.map);
			}
			return true;
		}

		public string DebugString()
		{
			return string.Concat(new object[]
			{
				"DesiredTotalAnimalWeight: ",
				this.DesiredTotalAnimalWeight,
				"\nCurrentTotalAnimalWeight: ",
				this.CurrentTotalAnimalWeight,
				"\nDesiredAnimalDensity: ",
				this.DesiredAnimalDensity
			});
		}
	}
}
