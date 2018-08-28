using System;
using UnityEngine;

namespace Verse
{
	public sealed class IngredientCount
	{
		public ThingFilter filter = new ThingFilter();

		private float count = 1f;

		public bool IsFixedIngredient
		{
			get
			{
				return this.filter.AllowedDefCount == 1;
			}
		}

		public ThingDef FixedIngredient
		{
			get
			{
				if (!this.IsFixedIngredient)
				{
					Log.Error("Called for SingleIngredient on an IngredientCount that is not IsSingleIngredient: " + this, false);
				}
				return this.filter.AnyAllowedDef;
			}
		}

		public string Summary
		{
			get
			{
				return this.count + "x " + this.filter.Summary;
			}
		}

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
			return "(" + this.Summary + ")";
		}
	}
}
