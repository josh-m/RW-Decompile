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
			float mass;
			if (this.TryGetValue(req, out mass))
			{
				return "StatsReport_GearAndInventoryMass".Translate() + ": " + mass.ToStringMassOffset();
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, new Func<Pawn, float>(MassUtility.GearAndInventoryMass), (ThingDef x) => 0f, out value);
		}
	}
}
