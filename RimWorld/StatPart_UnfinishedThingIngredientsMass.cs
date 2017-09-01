using System;
using Verse;

namespace RimWorld
{
	public class StatPart_UnfinishedThingIngredientsMass : StatPart
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
				return "StatsReport_IngredientsMass".Translate() + ": " + mass.ToStringMassOffset();
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			UnfinishedThing unfinishedThing = req.Thing as UnfinishedThing;
			if (unfinishedThing == null)
			{
				value = 0f;
				return false;
			}
			float num = 0f;
			for (int i = 0; i < unfinishedThing.ingredients.Count; i++)
			{
				num += unfinishedThing.ingredients[i].GetStatValue(StatDefOf.Mass, true) * (float)unfinishedThing.ingredients[i].stackCount;
			}
			value = num;
			return true;
		}
	}
}
