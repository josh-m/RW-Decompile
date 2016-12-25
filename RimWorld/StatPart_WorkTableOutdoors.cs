using System;
using Verse;

namespace RimWorld
{
	public class StatPart_WorkTableOutdoors : StatPart
	{
		public const float WorkRateFactor = 0.8f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && StatPart_WorkTableOutdoors.Applies(req.Thing))
			{
				val *= 0.8f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && StatPart_WorkTableOutdoors.Applies(req.Thing))
			{
				return "Outdoors".Translate() + ": x" + 0.8f.ToStringPercent();
			}
			return null;
		}

		public static bool Applies(Thing t)
		{
			return StatPart_WorkTableOutdoors.Applies(t.def, t.Map, t.Position);
		}

		public static bool Applies(ThingDef def, Map map, IntVec3 c)
		{
			if (def.building == null || !def.building.workSpeedPenaltyOutdoors)
			{
				return false;
			}
			if (map == null)
			{
				return false;
			}
			Room room = c.GetRoom(map);
			return room != null && room.PsychologicallyOutdoors;
		}
	}
}
