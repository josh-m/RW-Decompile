using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_WatchBuilding : JoyGiver_InteractBuilding
	{
		protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
		{
			if (!base.CanInteractWith(pawn, t, inBed))
			{
				return false;
			}
			if (inBed)
			{
				Building_Bed layingDownBed = pawn.jobs.curDriver.layingDownBed;
				return WatchBuildingUtility.CanWatchFromBed(pawn, layingDownBed, t);
			}
			return true;
		}

		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			IntVec3 c;
			Building t2;
			if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
			{
				return null;
			}
			return new Job(this.def.jobDef, t, c, t2);
		}
	}
}
