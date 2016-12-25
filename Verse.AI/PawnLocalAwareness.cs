using System;

namespace Verse.AI
{
	public static class PawnLocalAwareness
	{
		private const float SightRadius = 30f;

		public static bool AnimalAwareOf(this Pawn p, Thing t)
		{
			return p.RaceProps.ToolUser || p.Faction != null || ((p.Position - t.Position).LengthHorizontalSquared <= 900f && p.GetRoom() == t.GetRoom() && GenSight.LineOfSight(p.Position, t.Position, p.Map, false));
		}
	}
}
