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
			return t.ingestible.nutrition;
		}

		public override string BillRequirementsDescription(IngredientCount ing)
		{
			return "BillRequiresNutrition".Translate(new object[]
			{
				ing.GetBaseCount()
			}) + " (" + ing.filter.Summary + ")";
		}
	}
}
