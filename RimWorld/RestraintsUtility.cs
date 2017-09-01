using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class RestraintsUtility
	{
		public static bool InRestraints(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			if (pawn.HostFaction == null)
			{
				return false;
			}
			Lord lord = pawn.GetLord();
			return lord == null || lord.LordJob == null || !lord.LordJob.NeverInRestraints;
		}

		public static bool ShouldShowRestraintsInfo(Pawn pawn)
		{
			return pawn.IsPrisonerOfColony && RestraintsUtility.InRestraints(pawn);
		}
	}
}
