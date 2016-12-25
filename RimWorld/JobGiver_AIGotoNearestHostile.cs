using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIGotoNearestHostile : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			float num = 3.40282347E+38f;
			Thing thing = null;
			List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				IAttackTarget attackTarget = potentialTargetsFor[i];
				if (!attackTarget.ThreatDisabled())
				{
					Thing thing2 = (Thing)attackTarget;
					float num2 = thing2.Position.DistanceToSquared(pawn.Position);
					if (num2 < num && pawn.CanReach(thing2, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						num = num2;
						thing = thing2;
					}
				}
			}
			if (thing != null)
			{
				return new Job(JobDefOf.Goto, thing)
				{
					checkOverrideOnExpire = true,
					expiryInterval = 500
				};
			}
			return null;
		}
	}
}
