using System;
using Verse;

namespace RimWorld
{
	public class StatPart_Food : StatPart
	{
		private float factorStarving = 1f;

		private float factorUrgentlyHungry = 1f;

		private float factorHungry = 1f;

		private float factorFed = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.needs.food != null)
				{
					val *= this.FoodMultiplier(pawn.needs.food.CurCategory);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.needs.food != null)
				{
					return pawn.needs.food.CurCategory.GetLabel() + ": x" + this.FoodMultiplier(pawn.needs.food.CurCategory).ToStringPercent();
				}
			}
			return null;
		}

		private float FoodMultiplier(HungerCategory hunger)
		{
			switch (hunger)
			{
			case HungerCategory.Fed:
				return this.factorFed;
			case HungerCategory.Hungry:
				return this.factorHungry;
			case HungerCategory.UrgentlyHungry:
				return this.factorUrgentlyHungry;
			case HungerCategory.Starving:
				return this.factorStarving;
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
