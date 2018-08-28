using System;
using Verse;

namespace RimWorld
{
	public class StatPart_IsCorpseFresh : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			float num;
			if (this.TryGetIsFreshFactor(req, out num))
			{
				val *= num;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			float num;
			if (this.TryGetIsFreshFactor(req, out num) && num != 1f)
			{
				return "StatsReport_NotFresh".Translate() + ": x" + num.ToStringPercent();
			}
			return null;
		}

		private bool TryGetIsFreshFactor(StatRequest req, out float factor)
		{
			if (!req.HasThing)
			{
				factor = 1f;
				return false;
			}
			Corpse corpse = req.Thing as Corpse;
			if (corpse == null)
			{
				factor = 1f;
				return false;
			}
			factor = ((corpse.GetRotStage() != RotStage.Fresh) ? 0f : 1f);
			return true;
		}
	}
}
