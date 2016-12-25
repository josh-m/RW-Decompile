using System;
using Verse;

namespace RimWorld
{
	public class StatPart_WorkTableTemperature : StatPart
	{
		public const float WorkRateFactor = 0.6f;

		public const float MinTemp = 5f;

		public const float MaxTemp = 35f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && StatPart_WorkTableTemperature.Applies(req.Thing))
			{
				val *= 0.6f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && StatPart_WorkTableTemperature.Applies(req.Thing))
			{
				return "BadTemperature".Translate().CapitalizeFirst() + ": x" + 0.6f.ToStringPercent();
			}
			return null;
		}

		public static bool Applies(Thing t)
		{
			return t.Spawned && StatPart_WorkTableTemperature.Applies(t.def, t.Map, t.Position);
		}

		public static bool Applies(ThingDef tDef, Map map, IntVec3 c)
		{
			if (map == null)
			{
				return false;
			}
			if (tDef.building == null || !tDef.building.workSpeedPenaltyTemperature)
			{
				return false;
			}
			float temperatureForCell = GenTemperature.GetTemperatureForCell(c, map);
			return temperatureForCell < 5f || temperatureForCell > 35f;
		}
	}
}
