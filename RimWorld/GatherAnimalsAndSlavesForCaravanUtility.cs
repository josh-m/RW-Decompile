using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GatherAnimalsAndSlavesForCaravanUtility
	{
		public static bool IsFollowingAnyone(Pawn p)
		{
			return p.mindState.duty.focus.HasThing;
		}

		public static void SetFollower(Pawn p, Pawn follower)
		{
			p.mindState.duty.focus = follower;
		}

		public static void CheckArrived(Lord lord, IntVec3 meetingPoint, string memo, Predicate<Pawn> shouldCheckIfArrived, Predicate<Pawn> extraValidator = null)
		{
			bool flag = true;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (shouldCheckIfArrived(pawn))
				{
					if (!pawn.Position.InHorDistOf(meetingPoint, 10f) || !pawn.CanReach(meetingPoint, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn) || (extraValidator != null && !extraValidator(pawn)))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				lord.ReceiveMemo(memo);
			}
		}
	}
}
