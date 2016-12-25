using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class CompIngredients : ThingComp
	{
		private const int MaxNumIngredients = 3;

		public List<ThingDef> ingredients = new List<ThingDef>();

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.LookList<ThingDef>(ref this.ingredients, "ingredients", LookMode.Def, new object[0]);
		}

		public void RegisterIngredient(ThingDef def)
		{
			if (!this.ingredients.Contains(def))
			{
				this.ingredients.Add(def);
			}
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			if (piece != this.parent)
			{
				CompIngredients compIngredients = piece.TryGetComp<CompIngredients>();
				for (int i = 0; i < this.ingredients.Count; i++)
				{
					compIngredients.ingredients.Add(this.ingredients[i]);
				}
			}
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			base.PreAbsorbStack(otherStack, count);
			CompIngredients compIngredients = otherStack.TryGetComp<CompIngredients>();
			List<ThingDef> list = compIngredients.ingredients;
			for (int i = 0; i < list.Count; i++)
			{
				if (!this.ingredients.Contains(list[i]))
				{
					this.ingredients.Add(list[i]);
				}
			}
			if (this.ingredients.Count > 3)
			{
				this.ingredients.Shuffle<ThingDef>();
				while (this.ingredients.Count > 3)
				{
					this.ingredients.Remove(this.ingredients[this.ingredients.Count - 1]);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.ingredients.Count > 0)
			{
				stringBuilder.Append("Ingredients".Translate() + ": ");
				for (int i = 0; i < this.ingredients.Count; i++)
				{
					stringBuilder.Append(this.ingredients[i].label);
					if (i < this.ingredients.Count - 1)
					{
						stringBuilder.Append(", ");
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
