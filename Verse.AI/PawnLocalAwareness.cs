using System;

namespace Verse.AI
{
	public static class PawnLocalAwareness
	{
		private const float SightRadius = 30f;

		public static bool AnimalAwareOf(this Pawn p, Thing t)
		{
			return p.RaceProps.ToolUser || p.Faction != null || ((float)(p.Position - t.Position).LengthHorizontalSquared <= 900f && p.GetRoom(RegionType.Set_Passable) == t.GetRoom(RegionType.Set_Passable) && GenSight.LineOfSight(p.Position, t.Position, p.Map, false, null, 0, 0));
		}
	}
}
