using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WildPlantSpawner : IExposable
	{
		private Map map;

		private int cycleIndex;

		private float calculatedWholeMapNumDesiredPlants;

		private float calculatedWholeMapNumDesiredPlantsTmp;

		private int calculatedWholeMapNumNonZeroFertilityCells;

		private int calculatedWholeMapNumNonZeroFertilityCellsTmp;

		private bool hasWholeMapNumDesiredPlantsCalculated;

		private float? cachedCavePlantsCommonalitiesSum;

		private static List<ThingDef> allCavePlants = new List<ThingDef>();

		private static List<ThingDef> tmpPossiblePlants = new List<ThingDef>();

		private static List<KeyValuePair<ThingDef, float>> tmpPossiblePlantsWithWeight = new List<KeyValuePair<ThingDef, float>>();

		private static Dictionary<ThingDef, float> distanceSqToNearbyClusters = new Dictionary<ThingDef, float>();

		private static Dictionary<ThingDef, List<float>> nearbyClusters = new Dictionary<ThingDef, List<float>>();

		private static List<KeyValuePair<ThingDef, List<float>>> nearbyClustersList = new List<KeyValuePair<ThingDef, List<float>>>();

		private const float CavePlantsDensityFactor = 0.5f;

		private const int PlantSaturationScanRadius = 20;

		private const float MapFractionCheckPerTick = 0.0001f;

		private const float ChanceToRegrow = 0.012f;

		private const float CavePlantChanceToRegrow = 0.0001f;

		private const float BaseLowerOrderScanRadius = 7f;

		private const float LowerOrderScanRadiusWildClusterRadiusFactor = 1.5f;

		private const float MinDesiredLowerOrderPlantsToConsiderSkipping = 4f;

		private const float MinLowerOrderPlantsPct = 0.57f;

		private const float LocalPlantProportionsMaxScanRadius = 25f;

		private const float MaxLocalProportionsBias = 7f;

		private const float CavePlantRegrowDays = 130f;

		private static readonly SimpleCurve GlobalPctSelectionWeightBias = new SimpleCurve
		{
			{
				new CurvePoint(0f, 3f),
				true
			},
			{
				new CurvePoint(1f, 1f),
				true
			},
			{
				new CurvePoint(1.5f, 0.25f),
				true
			},
			{
				new CurvePoint(3f, 0.02f),
				true
			}
		};

		private static List<ThingDef> tmpPlantDefsLowerOrder = new List<ThingDef>();

		public float CurrentPlantDensity
		{
			get
			{
				return this.map.Biome.plantDensity * this.map.gameConditionManager.AggregatePlantDensityFactor(this.map);
			}
		}

		public float CurrentWholeMapNumDesiredPlants
		{
			get
			{
				CellRect cellRect = CellRect.WholeMap(this.map);
				float currentPlantDensity = this.CurrentPlantDensity;
				float num = 0f;
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					num += this.GetDesiredPlantsCountAt(iterator.Current, iterator.Current, currentPlantDensity);
					iterator.MoveNext();
				}
				return num;
			}
		}

		public int CurrentWholeMapNumNonZeroFertilityCells
		{
			get
			{
				CellRect cellRect = CellRect.WholeMap(this.map);
				int num = 0;
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					if (iterator.Current.GetTerrain(this.map).fertility > 0f)
					{
						num++;
					}
					iterator.MoveNext();
				}
				return num;
			}
		}

		public float CavePlantsCommonalitiesSum
		{
			get
			{
				float? num = this.cachedCavePlantsCommonalitiesSum;
				if (!num.HasValue)
				{
					this.cachedCavePlantsCommonalitiesSum = new float?(0f);
					for (int i = 0; i < WildPlantSpawner.allCavePlants.Count; i++)
					{
						float? num2 = this.cachedCavePlantsCommonalitiesSum;
						this.cachedCavePlantsCommonalitiesSum = ((!num2.HasValue) ? null : new float?(num2.GetValueOrDefault() + this.GetCommonalityOfPlant(WildPlantSpawner.allCavePlants[i])));
					}
				}
				return this.cachedCavePlantsCommonalitiesSum.Value;
			}
		}

		public WildPlantSpawner(Map map)
		{
			this.map = map;
		}

		public static void ResetStaticData()
		{
			WildPlantSpawner.allCavePlants.Clear();
			WildPlantSpawner.allCavePlants.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.category == ThingCategory.Plant && x.plant.cavePlant
			select x);
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.cycleIndex, "cycleIndex", 0, false);
			Scribe_Values.Look<float>(ref this.calculatedWholeMapNumDesiredPlants, "calculatedWholeMapNumDesiredPlants", 0f, false);
			Scribe_Values.Look<float>(ref this.calculatedWholeMapNumDesiredPlantsTmp, "calculatedWholeMapNumDesiredPlantsTmp", 0f, false);
			Scribe_Values.Look<bool>(ref this.hasWholeMapNumDesiredPlantsCalculated, "hasWholeMapNumDesiredPlantsCalculated", true, false);
			Scribe_Values.Look<int>(ref this.calculatedWholeMapNumNonZeroFertilityCells, "calculatedWholeMapNumNonZeroFertilityCells", 0, false);
			Scribe_Values.Look<int>(ref this.calculatedWholeMapNumNonZeroFertilityCellsTmp, "calculatedWholeMapNumNonZeroFertilityCellsTmp", 0, false);
		}

		public void WildPlantSpawnerTick()
		{
			if (DebugSettings.fastEcology || DebugSettings.fastEcologyRegrowRateOnly)
			{
				for (int i = 0; i < 2000; i++)
				{
					this.WildPlantSpawnerTickInternal();
				}
			}
			else
			{
				this.WildPlantSpawnerTickInternal();
			}
		}

		private void WildPlantSpawnerTickInternal()
		{
			int area = this.map.Area;
			int num = Mathf.CeilToInt((float)area * 0.0001f);
			float currentPlantDensity = this.CurrentPlantDensity;
			if (!this.hasWholeMapNumDesiredPlantsCalculated)
			{
				this.calculatedWholeMapNumDesiredPlants = this.CurrentWholeMapNumDesiredPlants;
				this.calculatedWholeMapNumNonZeroFertilityCells = this.CurrentWholeMapNumNonZeroFertilityCells;
				this.hasWholeMapNumDesiredPlantsCalculated = true;
			}
			int num2 = Mathf.CeilToInt(10000f);
			float chance = this.calculatedWholeMapNumDesiredPlants / (float)this.calculatedWholeMapNumNonZeroFertilityCells;
			for (int i = 0; i < num; i++)
			{
				if (this.cycleIndex >= area)
				{
					this.calculatedWholeMapNumDesiredPlants = this.calculatedWholeMapNumDesiredPlantsTmp;
					this.calculatedWholeMapNumDesiredPlantsTmp = 0f;
					this.calculatedWholeMapNumNonZeroFertilityCells = this.calculatedWholeMapNumNonZeroFertilityCellsTmp;
					this.calculatedWholeMapNumNonZeroFertilityCellsTmp = 0;
					this.cycleIndex = 0;
				}
				IntVec3 intVec = this.map.cellsInRandomOrder.Get(this.cycleIndex);
				this.calculatedWholeMapNumDesiredPlantsTmp += this.GetDesiredPlantsCountAt(intVec, intVec, currentPlantDensity);
				if (intVec.GetTerrain(this.map).fertility > 0f)
				{
					this.calculatedWholeMapNumNonZeroFertilityCellsTmp++;
				}
				float mtb = (!this.GoodRoofForCavePlant(intVec)) ? this.map.Biome.wildPlantRegrowDays : 130f;
				if (Rand.Chance(chance) && Rand.MTBEventOccurs(mtb, 60000f, (float)num2) && this.CanRegrowAt(intVec))
				{
					this.CheckSpawnWildPlantAt(intVec, currentPlantDensity, this.calculatedWholeMapNumDesiredPlants, false);
				}
				this.cycleIndex++;
			}
		}

		public bool CheckSpawnWildPlantAt(IntVec3 c, float plantDensity, float wholeMapNumDesiredPlants, bool setRandomGrowth = false)
		{
			if (plantDensity <= 0f || c.GetPlant(this.map) != null || c.GetCover(this.map) != null || c.GetEdifice(this.map) != null || this.map.fertilityGrid.FertilityAt(c) <= 0f || !PlantUtility.SnowAllowsPlanting(c, this.map))
			{
				return false;
			}
			bool cavePlants = this.GoodRoofForCavePlant(c);
			if (this.SaturatedAt(c, plantDensity, cavePlants, wholeMapNumDesiredPlants))
			{
				return false;
			}
			this.CalculatePlantsWhichCanGrowAt(c, WildPlantSpawner.tmpPossiblePlants, cavePlants, plantDensity);
			if (!WildPlantSpawner.tmpPossiblePlants.Any<ThingDef>())
			{
				return false;
			}
			this.CalculateDistancesToNearbyClusters(c);
			WildPlantSpawner.tmpPossiblePlantsWithWeight.Clear();
			for (int i = 0; i < WildPlantSpawner.tmpPossiblePlants.Count; i++)
			{
				float value = this.PlantChoiceWeight(WildPlantSpawner.tmpPossiblePlants[i], c, WildPlantSpawner.distanceSqToNearbyClusters, wholeMapNumDesiredPlants, plantDensity);
				WildPlantSpawner.tmpPossiblePlantsWithWeight.Add(new KeyValuePair<ThingDef, float>(WildPlantSpawner.tmpPossiblePlants[i], value));
			}
			KeyValuePair<ThingDef, float> keyValuePair;
			if (!WildPlantSpawner.tmpPossiblePlantsWithWeight.TryRandomElementByWeight((KeyValuePair<ThingDef, float> x) => x.Value, out keyValuePair))
			{
				return false;
			}
			Plant plant = (Plant)ThingMaker.MakeThing(keyValuePair.Key, null);
			if (setRandomGrowth)
			{
				plant.Growth = Rand.Range(0.07f, 1f);
				if (plant.def.plant.LimitedLifespan)
				{
					plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
				}
			}
			GenSpawn.Spawn(plant, c, this.map, WipeMode.Vanish);
			return true;
		}

		private float PlantChoiceWeight(ThingDef plantDef, IntVec3 c, Dictionary<ThingDef, float> distanceSqToNearbyClusters, float wholeMapNumDesiredPlants, float plantDensity)
		{
			float commonalityOfPlant = this.GetCommonalityOfPlant(plantDef);
			float commonalityPctOfPlant = this.GetCommonalityPctOfPlant(plantDef);
			float num = commonalityOfPlant;
			if (num <= 0f)
			{
				return num;
			}
			float num2 = 0.5f;
			if ((float)this.map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count > wholeMapNumDesiredPlants / 2f && !plantDef.plant.cavePlant)
			{
				num2 = (float)this.map.listerThings.ThingsOfDef(plantDef).Count / (float)this.map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count / commonalityPctOfPlant;
				num *= WildPlantSpawner.GlobalPctSelectionWeightBias.Evaluate(num2);
			}
			if (plantDef.plant.GrowsInClusters && num2 < 1.1f)
			{
				float num3 = (!plantDef.plant.cavePlant) ? this.map.Biome.PlantCommonalitiesSum : this.CavePlantsCommonalitiesSum;
				float x = commonalityOfPlant * plantDef.plant.wildClusterWeight / (num3 - commonalityOfPlant + commonalityOfPlant * plantDef.plant.wildClusterWeight);
				float num4 = 1f / (3.14159274f * (float)plantDef.plant.wildClusterRadius * (float)plantDef.plant.wildClusterRadius);
				num4 = GenMath.LerpDoubleClamped(commonalityPctOfPlant, 1f, 1f, num4, x);
				float f;
				if (distanceSqToNearbyClusters.TryGetValue(plantDef, out f))
				{
					float x2 = Mathf.Sqrt(f);
					num *= GenMath.LerpDoubleClamped((float)plantDef.plant.wildClusterRadius * 0.9f, (float)plantDef.plant.wildClusterRadius * 1.1f, plantDef.plant.wildClusterWeight, num4, x2);
				}
				else
				{
					num *= num4;
				}
			}
			if (plantDef.plant.wildEqualLocalDistribution)
			{
				float f2 = wholeMapNumDesiredPlants * commonalityPctOfPlant;
				float num5 = (float)Mathf.Max(this.map.Size.x, this.map.Size.z) / Mathf.Sqrt(f2);
				float num6 = num5 * 2f;
				if (plantDef.plant.GrowsInClusters)
				{
					num6 = Mathf.Max(num6, (float)plantDef.plant.wildClusterRadius * 1.6f);
				}
				num6 = Mathf.Max(num6, 7f);
				if (num6 <= 25f)
				{
					num *= this.LocalPlantProportionsWeightFactor(c, commonalityPctOfPlant, plantDensity, num6, plantDef);
				}
			}
			return num;
		}

		private float LocalPlantProportionsWeightFactor(IntVec3 c, float commonalityPct, float plantDensity, float radiusToScan, ThingDef plantDef)
		{
			float numDesiredPlantsLocally = 0f;
			int numPlants = 0;
			int numPlantsThisDef = 0;
			RegionTraverser.BreadthFirstTraverse(c, this.map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), radiusToScan), delegate(Region reg)
			{
				numDesiredPlantsLocally += this.GetDesiredPlantsCountIn(reg, c, plantDensity);
				numPlants += reg.ListerThings.ThingsInGroup(ThingRequestGroup.Plant).Count;
				numPlantsThisDef += reg.ListerThings.ThingsOfDef(plantDef).Count;
				return false;
			}, 999999, RegionType.Set_Passable);
			float num = numDesiredPlantsLocally * commonalityPct;
			if (num < 2f)
			{
				return 1f;
			}
			if ((float)numPlants <= numDesiredPlantsLocally * 0.5f)
			{
				return 1f;
			}
			float t = (float)numPlantsThisDef / (float)numPlants / commonalityPct;
			return Mathf.Lerp(7f, 1f, t);
		}

		private void CalculatePlantsWhichCanGrowAt(IntVec3 c, List<ThingDef> outPlants, bool cavePlants, float plantDensity)
		{
			outPlants.Clear();
			if (cavePlants)
			{
				for (int i = 0; i < WildPlantSpawner.allCavePlants.Count; i++)
				{
					if (WildPlantSpawner.allCavePlants[i].CanEverPlantAt(c, this.map))
					{
						outPlants.Add(WildPlantSpawner.allCavePlants[i]);
					}
				}
			}
			else
			{
				List<ThingDef> allWildPlants = this.map.Biome.AllWildPlants;
				for (int j = 0; j < allWildPlants.Count; j++)
				{
					ThingDef thingDef = allWildPlants[j];
					if (thingDef.CanEverPlantAt(c, this.map))
					{
						if (thingDef.plant.wildOrder != this.map.Biome.LowestWildAndCavePlantOrder)
						{
							float num = 7f;
							if (thingDef.plant.GrowsInClusters)
							{
								num = Math.Max(num, (float)thingDef.plant.wildClusterRadius * 1.5f);
							}
							if (!this.EnoughLowerOrderPlantsNearby(c, plantDensity, num, thingDef))
							{
								goto IL_FF;
							}
						}
						outPlants.Add(thingDef);
					}
					IL_FF:;
				}
			}
		}

		private bool EnoughLowerOrderPlantsNearby(IntVec3 c, float plantDensity, float radiusToScan, ThingDef plantDef)
		{
			float num = 0f;
			WildPlantSpawner.tmpPlantDefsLowerOrder.Clear();
			List<ThingDef> allWildPlants = this.map.Biome.AllWildPlants;
			for (int i = 0; i < allWildPlants.Count; i++)
			{
				if (allWildPlants[i].plant.wildOrder < plantDef.plant.wildOrder)
				{
					num += this.GetCommonalityPctOfPlant(allWildPlants[i]);
					WildPlantSpawner.tmpPlantDefsLowerOrder.Add(allWildPlants[i]);
				}
			}
			float numDesiredPlantsLocally = 0f;
			int numPlantsLowerOrder = 0;
			RegionTraverser.BreadthFirstTraverse(c, this.map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), radiusToScan), delegate(Region reg)
			{
				numDesiredPlantsLocally += this.GetDesiredPlantsCountIn(reg, c, plantDensity);
				for (int j = 0; j < WildPlantSpawner.tmpPlantDefsLowerOrder.Count; j++)
				{
					numPlantsLowerOrder += reg.ListerThings.ThingsOfDef(WildPlantSpawner.tmpPlantDefsLowerOrder[j]).Count;
				}
				return false;
			}, 999999, RegionType.Set_Passable);
			float num2 = numDesiredPlantsLocally * num;
			return num2 < 4f || (float)numPlantsLowerOrder / num2 >= 0.57f;
		}

		private bool SaturatedAt(IntVec3 c, float plantDensity, bool cavePlants, float wholeMapNumDesiredPlants)
		{
			int num = GenRadial.NumCellsInRadius(20f);
			float num2 = wholeMapNumDesiredPlants * ((float)num / (float)this.map.Area);
			if (num2 <= 4f || !this.map.Biome.wildPlantsCareAboutLocalFertility)
			{
				return (float)this.map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count >= wholeMapNumDesiredPlants;
			}
			float numDesiredPlantsLocally = 0f;
			int numPlants = 0;
			RegionTraverser.BreadthFirstTraverse(c, this.map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), 20f), delegate(Region reg)
			{
				numDesiredPlantsLocally += this.GetDesiredPlantsCountIn(reg, c, plantDensity);
				numPlants += reg.ListerThings.ThingsInGroup(ThingRequestGroup.Plant).Count;
				return false;
			}, 999999, RegionType.Set_Passable);
			return (float)numPlants >= numDesiredPlantsLocally;
		}

		private void CalculateDistancesToNearbyClusters(IntVec3 c)
		{
			WildPlantSpawner.nearbyClusters.Clear();
			WildPlantSpawner.nearbyClustersList.Clear();
			int num = GenRadial.NumCellsInRadius((float)(this.map.Biome.MaxWildAndCavePlantsClusterRadius * 2));
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = c + GenRadial.RadialPattern[i];
				if (intVec.InBounds(this.map))
				{
					List<Thing> list = this.map.thingGrid.ThingsListAtFast(intVec);
					for (int j = 0; j < list.Count; j++)
					{
						Thing thing = list[j];
						if (thing.def.category == ThingCategory.Plant && thing.def.plant.GrowsInClusters)
						{
							float item = (float)intVec.DistanceToSquared(c);
							List<float> list2;
							if (!WildPlantSpawner.nearbyClusters.TryGetValue(thing.def, out list2))
							{
								list2 = SimplePool<List<float>>.Get();
								WildPlantSpawner.nearbyClusters.Add(thing.def, list2);
								WildPlantSpawner.nearbyClustersList.Add(new KeyValuePair<ThingDef, List<float>>(thing.def, list2));
							}
							list2.Add(item);
						}
					}
				}
			}
			WildPlantSpawner.distanceSqToNearbyClusters.Clear();
			for (int k = 0; k < WildPlantSpawner.nearbyClustersList.Count; k++)
			{
				List<float> value = WildPlantSpawner.nearbyClustersList[k].Value;
				value.Sort();
				WildPlantSpawner.distanceSqToNearbyClusters.Add(WildPlantSpawner.nearbyClustersList[k].Key, value[value.Count / 2]);
				value.Clear();
				SimplePool<List<float>>.Return(value);
			}
		}

		private bool CanRegrowAt(IntVec3 c)
		{
			return c.GetTemperature(this.map) > 0f && (!c.Roofed(this.map) || this.GoodRoofForCavePlant(c));
		}

		private bool GoodRoofForCavePlant(IntVec3 c)
		{
			RoofDef roof = c.GetRoof(this.map);
			return roof != null && roof.isNatural;
		}

		private float GetCommonalityOfPlant(ThingDef plant)
		{
			return (!plant.plant.cavePlant) ? this.map.Biome.CommonalityOfPlant(plant) : plant.plant.cavePlantWeight;
		}

		private float GetCommonalityPctOfPlant(ThingDef plant)
		{
			return (!plant.plant.cavePlant) ? this.map.Biome.CommonalityPctOfPlant(plant) : (this.GetCommonalityOfPlant(plant) / this.CavePlantsCommonalitiesSum);
		}

		public float GetBaseDesiredPlantsCountAt(IntVec3 c)
		{
			float num = c.GetTerrain(this.map).fertility;
			if (this.GoodRoofForCavePlant(c))
			{
				num *= 0.5f;
			}
			return num;
		}

		public float GetDesiredPlantsCountAt(IntVec3 c, IntVec3 forCell, float plantDensity)
		{
			return Mathf.Min(this.GetBaseDesiredPlantsCountAt(c) * plantDensity * forCell.GetTerrain(this.map).fertility, 1f);
		}

		public float GetDesiredPlantsCountIn(Region reg, IntVec3 forCell, float plantDensity)
		{
			return Mathf.Min(reg.GetBaseDesiredPlantsCount(true) * plantDensity * forCell.GetTerrain(this.map).fertility, (float)reg.CellCount);
		}
	}
}
