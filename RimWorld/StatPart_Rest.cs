using System;
using Verse;

namespace RimWorld
{
	public class StatPart_Rest : StatPart
	{
		private float factorExhausted = 1f;

		private float factorVeryTired = 1f;

		private float factorTired = 1f;

		private float factorRested = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.needs.rest != null)
				{
					val *= this.RestMultiplier(pawn.needs.rest.CurCategory);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.needs.rest != null)
				{
					return pawn.needs.rest.CurCategory.GetLabel() + ": x" + this.RestMultiplier(pawn.needs.rest.CurCategory).ToStringPercent();
				}
			}
			return null;
		}

		private float RestMultiplier(RestCategory fatigue)
		{
			switch (fatigue)
			{
			case RestCategory.Rested:
				return this.factorRested;
			case RestCategory.Tired:
				return this.factorTired;
			case RestCategory.VeryTired:
				return this.factorVeryTired;
			case RestCategory.Exhausted:
				return this.factorExhausted;
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
