using System;

namespace RimWorld
{
	[Flags]
	public enum FoodTypeFlags
	{
		None = 0,
		VegetableOrFruit = 1,
		Meat = 2,
		Fluid = 4,
		Corpse = 8,
		Seed = 16,
		AnimalProduct = 32,
		Plant = 64,
		Tree = 128,
		Meal = 256,
		Processed = 512,
		Liquor = 1024,
		Kibble = 2048,
		VegetarianAnimal = 3857,
		VegetarianRoughAnimal = 3921,
		CarnivoreAnimal = 2826,
		CarnivoreAnimalStrict = 10,
		OmnivoreAnimal = 3867,
		OmnivoreRoughAnimal = 3931,
		DendrovoreAnimal = 2705,
		OvivoreAnimal = 2848,
		OmnivoreHuman = 3903
	}
}
