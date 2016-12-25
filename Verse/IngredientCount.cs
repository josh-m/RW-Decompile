using System;
using UnityEngine;

namespace Verse
{
	public sealed class IngredientCount
	{
		public ThingFilter filter = new ThingFilter();

		private float count = 1f;

		public int CountRequiredOfFor(ThingDef thingDef, RecipeDef recipe)
		{
			float num = recipe.IngredientValueGetter.ValuePerUnitOf(thingDef);
			return Mathf.CeilToInt(this.count / num);
		}

		public float GetBaseCount()
		{
			return this.count;
		}

		public void SetBaseCount(float count)
		{
			this.count = count;
		}

		public void ResolveReferences()
		{
			this.filter.ResolveReferences();
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.count,
				"x ",
				this.filter.ToString(),
				")"
			});
		}
	}
}
