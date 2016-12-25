using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WildSpawner
	{
		private const int AnimalCheckInterval = 1210;

		private const float BaseAnimalSpawnChancePerInterval = 0.0268888883f;

		private const int PlantTrySpawnIntervalAt100EdgeLength = 650;

		private Map map;

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
				num *= this.map.mapConditionManager.AggregateAnimalDensityFactor();
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
				int num = (int)(10000f / desiredAnimalDensity);
				return (float)(this.map.Area / num);
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

		public void WildSpawnerTick()
		{
			IntVec3 loc;
			if (Find.TickManager.TicksGame % 1210 == 0 && !this.AnimalEcosystemFull && Rand.Value < 0.0268888883f * this.DesiredAnimalDensity && RCellFinder.TryFindRandomPawnEntryCell(out loc, this.map))
			{
				this.SpawnRandomWildAnimalAt(loc);
			}
			float num = this.map.mapConditionManager.AggregatePlantDensityFactor();
			if (num > 0.0001f)
			{
				int num2 = this.map.Size.x * 2 + this.map.Size.z * 2;
				int num3 = 650 / (num2 / 100);
				num3 = GenMath.RoundRandom((float)num3 / num);
				if (Find.TickManager.TicksGame % num3 == 0)
				{
					this.TrySpawnPlantFromMapEdge();
				}
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

		public void SpawnRandomWildAnimalAt(IntVec3 loc)
		{
			PawnKindDef pawnKindDef = (from a in this.map.Biome.AllWildAnimals
			where this.map.mapTemperature.SeasonAcceptableFor(a.race)
			select a).RandomElementByWeight((PawnKindDef def) => this.map.Biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
			if (pawnKindDef == null)
			{
				Log.Error("No spawnable animals right now.");
				return;
			}
			int randomInRange = pawnKindDef.wildSpawn_GroupSizeRange.RandomInRange;
			int radius = Mathf.CeilToInt(Mathf.Sqrt((float)pawnKindDef.wildSpawn_GroupSizeRange.max));
			for (int i = 0; i < randomInRange; i++)
			{
				IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, this.map, radius);
				Pawn newThing = PawnGenerator.GeneratePawn(pawnKindDef, null);
				GenSpawn.Spawn(newThing, loc2, this.map);
			}
		}

		public string DebugString()
		{
			return string.Concat(new object[]
			{
				"CurrentTotalAnimalWeight: ",
				this.CurrentTotalAnimalWeight,
				"\nDesiredAnimalDensity: ",
				this.DesiredAnimalDensity,
				"\nDesiredTotalAnimalWeight: ",
				this.DesiredTotalAnimalWeight
			});
		}
	}
}
