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
				foreach (Designation des in Find.DesignationManager.DesignationsOfDef(this.DesDef))
				{
					yield return des.target.Cell;
				}
			}
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
			return pawn.Faction == Faction.OfPlayer && Find.DesignationManager.DesignationAt(c, this.DesDef) != null && pawn.CanReserveAndReach(c, PathEndMode.Touch, pawn.NormalMaxDanger(), 1);
		}
	}
}
