using System;
using Verse;

namespace RimWorld
{
	public class ThinkNode_ConditionalShouldFollowMaster : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
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
			return (master.Spawned || carriedBy != null) && (master.Drafted || (master.CurJob != null && master.CurJob.def == JobDefOf.Hunt) || (carriedBy != null && carriedBy.HostileTo(master)));
		}
	}
}
