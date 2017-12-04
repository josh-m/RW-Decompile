using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_CavePlants : GenStep
	{
		private const float PlantMinGrowth = 0.07f;

		private const float PlantChancePerCell = 0.18f;

		public override void Generate(Map map)
		{
			map.regionAndRoomUpdater.Enabled = false;
			MapGenFloatGrid caves = MapGenerator.Caves;
			List<ThingDef> source = (from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.category == ThingCategory.Plant && x.plant.cavePlant
			select x).ToList<ThingDef>();
			foreach (IntVec3 c in map.AllCells.InRandomOrder(null))
			{
				if (c.GetEdifice(map) == null && c.GetCover(map) == null && caves[c] > 0f && c.Roofed(map) && map.fertilityGrid.FertilityAt(c) > 0f)
				{
					if (Rand.Chance(0.18f))
					{
						IEnumerable<ThingDef> source2 = from def in source
						where def.CanEverPlantAt(c, map)
						select def;
						if (source2.Any<ThingDef>())
						{
							ThingDef thingDef = source2.RandomElement<ThingDef>();
							int randomInRange = thingDef.plant.wildClusterSizeRange.RandomInRange;
							for (int i = 0; i < randomInRange; i++)
							{
								IntVec3 c2;
								if (i == 0)
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
							}
						}
					}
				}
			}
			map.regionAndRoomUpdater.Enabled = true;
		}
	}
}
