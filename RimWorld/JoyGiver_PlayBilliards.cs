using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_PlayBilliards : JoyGiver_InteractBuilding
	{
		protected override bool CanDoDuringParty
		{
			get
			{
				return true;
			}
		}

		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			if (!JoyGiver_PlayBilliards.ThingHasStandableSpaceOnAllSides(t))
			{
				return null;
			}
			return new Job(this.def.jobDef, t);
		}

		public static bool ThingHasStandableSpaceOnAllSides(Thing t)
		{
			CellRect cellRect = t.OccupiedRect();
			CellRect.CellRectIterator iterator = cellRect.ExpandedBy(1).GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!cellRect.Contains(current))
				{
					if (!current.Standable(t.Map))
					{
						return false;
					}
				}
				iterator.MoveNext();
			}
			return true;
		}
	}
}
