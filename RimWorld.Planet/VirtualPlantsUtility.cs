using System;
using Verse;

namespace RimWorld.Planet
{
	public static class VirtualPlantsUtility
	{
		private static readonly FloatRange VirtualPlantNutritionRandomFactor = new FloatRange(0.7f, 1f);

		public static bool CanEverEatVirtualPlants(Pawn p)
		{
			return p.RaceProps.Eats(FoodTypeFlags.Plant);
		}

		public static bool CanEatVirtualPlantsNow(Pawn p)
		{
			return p.Tile >= 0 && !p.Dead && p.IsWorldPawn() && VirtualPlantsUtility.CanEverEatVirtualPlants(p) && VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(p.Tile);
		}

		public static bool EnvironmentAllowsEatingVirtualPlantsNowAt(int tile)
		{
			BiomeDef biome = Find.WorldGrid[tile].biome;
			if (!biome.hasVirtualPlants)
			{
				return false;
			}
			float temperatureAtTile = GenTemperature.GetTemperatureAtTile(tile);
			return temperatureAtTile >= 0f;
		}

		public static void EatVirtualPlants(Pawn p)
		{
			float num = ThingDefOf.PlantGrass.ingestible.nutrition * VirtualPlantsUtility.VirtualPlantNutritionRandomFactor.RandomInRange;
			p.needs.food.CurLevel += num;
		}
	}
}
