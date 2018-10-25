using RimWorld;
using System;

namespace Verse
{
	public class IngredientValueGetter_Nutrition : IngredientValueGetter
	{
		public override float ValuePerUnitOf(ThingDef t)
		{
			if (!t.IsNutritionGivingIngestible)
			{
				return 0f;
			}
			return t.GetStatValueAbstract(StatDefOf.Nutrition, null);
		}

		public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
		{
			return "BillRequiresNutrition".Translate(ing.GetBaseCount()) + " (" + ing.filter.Summary + ")";
		}
	}
}
