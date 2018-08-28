using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InteractBuildingSitAdjacent : JoyGiver_InteractBuilding
	{
		private static List<IntVec3> tmpCells = new List<IntVec3>();

		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			JoyGiver_InteractBuildingSitAdjacent.tmpCells.Clear();
			JoyGiver_InteractBuildingSitAdjacent.tmpCells.AddRange(GenAdjFast.AdjacentCellsCardinal(t));
			JoyGiver_InteractBuildingSitAdjacent.tmpCells.Shuffle<IntVec3>();
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < JoyGiver_InteractBuildingSitAdjacent.tmpCells.Count; j++)
				{
					IntVec3 c = JoyGiver_InteractBuildingSitAdjacent.tmpCells[j];
					if (!c.IsForbidden(pawn) && pawn.CanReserve(c, 1, -1, null, false))
					{
						if (i == 0)
						{
							Building edifice = c.GetEdifice(pawn.Map);
							if (edifice == null || !edifice.def.building.isSittable || !pawn.CanReserve(edifice, 1, -1, null, false))
							{
								goto IL_CE;
							}
						}
						return new Job(this.def.jobDef, t, c);
					}
					IL_CE:;
				}
				if (this.def.requireChair)
				{
					break;
				}
			}
			return null;
		}
	}
}
