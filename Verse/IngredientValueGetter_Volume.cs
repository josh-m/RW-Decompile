using System;
using System.Linq;

namespace Verse
{
	public class IngredientValueGetter_Volume : IngredientValueGetter
	{
		public override float ValuePerUnitOf(ThingDef t)
		{
			if (t.IsStuff)
			{
				return t.VolumePerUnit;
			}
			return 1f;
		}

		public override string BillRequirementsDescription(IngredientCount ing)
		{
			return "BillRequires".Translate(new object[]
			{
				ing.GetBaseCount(),
				ing.filter.Summary
			});
		}

		public override string ExtraDescriptionLine(RecipeDef r)
		{
			if (r.ingredients.Any((IngredientCount ing) => ing.filter.AllowedThingDefs.Any((ThingDef td) => td.smallVolume)))
			{
				return "BillRequiresMayVary".Translate();
			}
			return null;
		}
	}
}
