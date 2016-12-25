using System;
using Verse;

namespace RimWorld
{
	public static class BillUtility
	{
		public static void TryDrawIngredientSearchRadiusOnMap(this Bill bill, IntVec3 center)
		{
			if (bill.ingredientSearchRadius < GenRadial.MaxRadialPatternRadius)
			{
				GenDraw.DrawRadiusRing(center, bill.ingredientSearchRadius);
			}
		}

		public static Bill MakeNewBill(this RecipeDef recipe)
		{
			if (recipe.UsesUnfinishedThing)
			{
				return new Bill_ProductionWithUft(recipe);
			}
			return new Bill_Production(recipe);
		}
	}
}
