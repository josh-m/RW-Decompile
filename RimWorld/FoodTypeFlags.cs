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
		Seeds = 16,
		AnimalProduct = 32,
		Plant = 64,
		Tree = 128,
		Meal = 256,
		Processed = 512,
		Liquor = 1024,
		Kibble = 2048,
		VegetarianAnimal = 3345,
		VegetarianRoughAnimal = 3409,
		CarnivoreAnimal = 2314,
		CarnivoreAnimalStrict = 10,
		OmnivoreAnimal = 3355,
		OmnivoreRoughAnimal = 3419,
		DendrovoreAnimal = 2193,
		OvivoreAnimal = 2336,
		OmnivoreHuman = 1855
	}
}
