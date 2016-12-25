using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_SpaciousInterior : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.space == null || p.needs.space.CurCategory != SpaceCategory.Spacious)
			{
				return false;
			}
			Building edifice = p.Position.GetEdifice();
			if (edifice != null && edifice is Building_Door)
			{
				return false;
			}
			Room room = p.GetRoom();
			return room != null && !room.TouchesMapEdge;
		}
	}
}
