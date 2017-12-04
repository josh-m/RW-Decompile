using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StatPart_Glow : StatPart
	{
		private bool humanlikeOnly;

		private SimpleCurve factorFromGlowCurve;

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			if (this.factorFromGlowCurve == null)
			{
				yield return "factorFromLightCurve is null.";
			}
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && this.ActiveFor(req.Thing))
			{
				val *= this.FactorFromGlow(req.Thing);
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && this.ActiveFor(req.Thing))
			{
				return "StatsReport_LightMultiplier".Translate(new object[]
				{
					this.GlowLevel(req.Thing).ToStringPercent()
				}) + ": x" + this.FactorFromGlow(req.Thing).ToStringPercent();
			}
			return null;
		}

		private bool ActiveFor(Thing t)
		{
			if (this.humanlikeOnly)
			{
				Pawn pawn = t as Pawn;
				if (pawn != null && !pawn.RaceProps.Humanlike)
				{
					return false;
				}
			}
			return t.Spawned;
		}

		private float GlowLevel(Thing t)
		{
			return t.Map.glowGrid.GameGlowAt(t.Position, false);
		}

		private float FactorFromGlow(Thing t)
		{
			return this.factorFromGlowCurve.Evaluate(this.GlowLevel(t));
		}
	}
}
