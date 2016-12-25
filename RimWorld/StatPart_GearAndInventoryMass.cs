using System;
using Verse;

namespace RimWorld
{
	public class StatPart_GearAndInventoryMass : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			float num;
			if (this.TryGetValue(req, out num))
			{
				val += num;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			float num;
			if (this.TryGetValue(req, out num))
			{
				return "StatsReport_GearAndInventoryMass".Translate() + ": " + num.ToString("+0.##;-0.##;0") + "kg";
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => MassUtility.GearAndInventoryMass(x), (ThingDef x) => 0f, out value);
		}
	}
}
