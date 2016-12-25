using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_FoodPoisoningChance : CompProperties
	{
		public float chance = 0.02f;

		public CompProperties_FoodPoisoningChance()
		{
			this.compClass = typeof(CompFoodPoisoningChance);
		}
	}
}
