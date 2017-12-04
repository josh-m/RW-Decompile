using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_Plants : GenStep
	{
		private static Dictionary<ThingDef, int> numExtant = new Dictionary<ThingDef, int>();

		private static Dictionary<ThingDef, float> desiredProportions = new Dictionary<ThingDef, float>();

		private static int totalExtant = 0;

		private const float PlantMinGrowth = 0.07f;

		public override void Generate(Map map)
		{
			map.regionAndRoomUpdater.Enabled = false;
			List<ThingDef> list = map.Biome.AllWildPlants.ToList<ThingDef>();
			for (int i = 0; i < list.Count; i++)
			{
				GenStep_Plants.numExtant.Add(list[i], 0);
			}
			GenStep_Plants.desiredProportions = GenPlant.CalculateDesiredPlantProportions(map.Biome);
			MapGenFloatGrid caves = MapGenerator.Caves;
			float num = map.Biome.plantDensity * map.gameConditionManager.AggregatePlantDensityFactor();
			foreach (IntVec3 c in map.AllCells.InRandomOrder(null))
			{
				if (c.GetEdifice(map) == null && c.GetCover(map) == null && caves[c] <= 0f)
				{
					float num2 = map.fertilityGrid.FertilityAt(c);
					float num3 = num2 * num;
					if (Rand.Value < num3)
					{
						IEnumerable<ThingDef> source = from def in list
						where def.CanEverPlantAt(c, map)
						select def;
						if (source.Any<ThingDef>())
						{
							ThingDef thingDef = source.RandomElementByWeight((ThingDef x) => GenStep_Plants.PlantChoiceWeight(x, map));
							int randomInRange = thingDef.plant.wildClusterSizeRange.RandomInRange;
							for (int j = 0; j < randomInRange; j++)
							{
								IntVec3 c2;
								if (j == 0)
								{
									c2 = c;
								}
								else if (!GenPlantReproduction.TryFindReproductionDestination(c, thingDef, SeedTargFindMode.MapGenCluster, map, out c2))
								{
									break;
								}
								Plant plant = (Plant)ThingMaker.MakeThing(thingDef, null);
								plant.Growth = Rand.Range(0.07f, 1f);
								if (plant.def.plant.LimitedLifespan)
								{
									plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
								}
								GenSpawn.Spawn(plant, c2, map);
								GenStep_Plants.RecordAdded(thingDef);
							}
						}
					}
				}
			}
			GenStep_Plants.numExtant.Clear();
			GenStep_Plants.desiredProportions.Clear();
			GenStep_Plants.totalExtant = 0;
			map.regionAndRoomUpdater.Enabled = true;
		}

		private static float PlantChoiceWeight(ThingDef def, Map map)
		{
			float num = map.Biome.CommonalityOfPlant(def);
			if (GenStep_Plants.totalExtant > 100)
			{
				float num2 = (float)GenStep_Plants.numExtant[def] / (float)GenStep_Plants.totalExtant;
				if (num2 < GenStep_Plants.desiredProportions[def] * 0.8f)
				{
					num *= 4f;
				}
			}
			return num / def.plant.wildClusterSizeRange.Average;
		}

		private static void RecordAdded(ThingDef plantDef)
		{
			GenStep_Plants.totalExtant++;
			Dictionary<ThingDef, int> dictionary;
			(dictionary = GenStep_Plants.numExtant)[plantDef] = dictionary[plantDef] + 1;
		}
	}
}
