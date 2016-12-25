using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_BuildRoof : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			return pawn.Map.areaManager.BuildRoof.ActiveCells;
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
			return pawn.Map.areaManager.BuildRoof[c] && !c.Roofed(pawn.Map) && pawn.CanReserve(c, 1) && (pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn) || this.BuildingToTouchToBeAbleToBuildRoof(c, pawn) != null) && RoofCollapseUtility.WithinRangeOfRoofHolder(c, pawn.Map) && RoofCollapseUtility.ConnectedToRoofHolder(c, pawn.Map, true);
		}

		private Building BuildingToTouchToBeAbleToBuildRoof(IntVec3 c, Pawn pawn)
		{
			if (c.Standable(pawn.Map))
			{
				return null;
			}
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice == null)
			{
				return null;
			}
			if (!pawn.CanReach(edifice, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn))
			{
				return null;
			}
			return edifice;
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c)
		{
			LocalTargetInfo targetB = c;
			if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), false, TraverseMode.ByPawn))
			{
				targetB = this.BuildingToTouchToBeAbleToBuildRoof(c, pawn);
			}
			return new Job(JobDefOf.BuildRoof, c, targetB);
		}
	}
}
