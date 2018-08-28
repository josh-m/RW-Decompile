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

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (!c.IsForbidden(pawn) && pawn.Map.designationManager.DesignationAt(c, this.DesDef) != null)
			{
				LocalTargetInfo target = c;
				ReservationLayerDef floor = ReservationLayerDefOf.Floor;
				if (pawn.CanReserve(target, 1, -1, floor, forced))
				{
					return true;
				}
			}
			return false;
		}
	}
}
