using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ClearSnow : WorkGiver_Scanner
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
			return Find.AreaSnowClear.ActiveCells;
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return Find.AreaSnowClear.TrueCount == 0;
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
			return Find.SnowGrid.GetDepth(c) >= 0.2f && pawn.CanReserveAndReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), 1);
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c)
		{
			return new Job(JobDefOf.ClearSnow, c);
		}
	}
}
