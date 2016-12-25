using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_SharedBedroom : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.ownership.OwnedBed != null && p.ownership.OwnedRoom == null && !p.ownership.OwnedBed.GetRoom().PsychologicallyOutdoors;
		}
	}
}
