using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_ConstructAffectFloor : WorkGiver_Scanner
	{
		protected abstract DesignationDef DesDef
		{
			get;
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer)
			{
				foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(this.DesDef))
				{
					yield return des.target.Cell;
				}
			}
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
			if (pawn.Map.designationManager.DesignationAt(c, this.DesDef) != null)
			{
				ReservationLayerDef floor = ReservationLayerDefOf.Floor;
				if (pawn.CanReserveAndReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, floor, false))
				{
					return true;
				}
			}
			return false;
		}
	}
}
