using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Miner : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation des in pawn.Map.designationManager.DesignationsOfDef(DesignationDefOf.Mine))
			{
				bool mayBeAccessible = false;
				for (int i = 0; i < 4; i++)
				{
					IntVec3 c = des.target.Cell + GenAdj.CardinalDirections[i];
					if (c.InBounds(pawn.Map) && c.Walkable(pawn.Map))
					{
						mayBeAccessible = true;
						break;
					}
				}
				if (mayBeAccessible)
				{
					Thing j = MineUtility.MineableInCell(des.target.Cell, pawn.Map);
					if (j != null)
					{
						yield return j;
					}
				}
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (!t.def.mineable)
			{
				return null;
			}
			if (pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) == null)
			{
				return null;
			}
			if (!pawn.CanReserve(t, 1))
			{
				return null;
			}
			bool flag = false;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = t.Position + GenAdj.AdjacentCells[i];
				if (c.InBounds(pawn.Map) && c.Standable(pawn.Map))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < 8; j++)
				{
					IntVec3 c2 = t.Position + GenAdj.AdjacentCells[j];
					if (c2.InBounds(t.Map))
					{
						if (c2.Walkable(t.Map) && !c2.Standable(t.Map))
						{
							Thing firstHaulable = c2.GetFirstHaulable(t.Map);
							if (firstHaulable != null && firstHaulable.def.passability == Traversability.PassThroughOnly)
							{
								return HaulAIUtility.HaulAsideJobFor(pawn, firstHaulable);
							}
						}
					}
				}
				return null;
			}
			return new Job(JobDefOf.Mine, t, 1500, true);
		}
	}
}
