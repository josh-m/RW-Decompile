using System;
using Verse;

namespace RimWorld
{
	public abstract class StatPart_Curve : StatPart
	{
		protected SimpleCurve curve;

		protected abstract bool AppliesTo(StatRequest req);

		protected abstract float CurveXGetter(StatRequest req);

		protected abstract string ExplanationLabel(StatRequest req);

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && this.AppliesTo(req))
			{
				val *= this.curve.Evaluate(this.CurveXGetter(req));
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && this.AppliesTo(req))
			{
				return this.ExplanationLabel(req) + ": x" + this.curve.Evaluate(this.CurveXGetter(req)).ToStringPercent();
			}
			return null;
		}
	}
}
