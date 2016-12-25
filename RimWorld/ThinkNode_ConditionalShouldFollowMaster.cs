using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalShouldFollowMaster : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn);
		}

		public static bool ShouldFollowMaster(Pawn pawn)
		{
			if (pawn.playerSettings == null)
			{
				return false;
			}
			Pawn master = pawn.playerSettings.master;
			if (master == null)
			{
				return false;
			}
			Pawn carriedBy = master.CarriedBy;
			if (!master.Spawned && carriedBy == null)
			{
				return false;
			}
			if (carriedBy != null && carriedBy.HostileTo(master))
			{
				return true;
			}
			if (master.Drafted)
			{
				return pawn.playerSettings.followDrafted;
			}
			return master.CurJob != null && (master.CurJob.def == JobDefOf.Hunt || master.CurJob.def == JobDefOf.Tame) && pawn.playerSettings.followFieldwork;
		}
	}
}
