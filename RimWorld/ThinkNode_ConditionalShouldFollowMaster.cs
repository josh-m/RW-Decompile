using System;
using Verse;
using Verse.AI;

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
			return (master.Spawned || carriedBy != null) && ((carriedBy != null && carriedBy.HostileTo(master)) || (pawn.playerSettings.followDrafted && master.Drafted) || (pawn.playerSettings.followFieldwork && master.mindState.lastJobTag == JobTag.Fieldwork));
		}
	}
}
