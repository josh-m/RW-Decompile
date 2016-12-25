using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InteractBuildingSitAdjacent : JoyGiver_InteractBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			Thing thing = null;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
				if (!c.IsForbidden(pawn))
				{
					Building edifice = c.GetEdifice(pawn.Map);
					if (edifice != null && edifice.def.building.isSittable && pawn.CanReserve(edifice, 1))
					{
						thing = edifice;
						break;
					}
				}
			}
			if (thing == null)
			{
				return null;
			}
			return new Job(this.def.jobDef, t, thing);
		}
	}
}
