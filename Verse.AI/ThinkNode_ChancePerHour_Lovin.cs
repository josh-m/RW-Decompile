using RimWorld;
using System;

namespace Verse.AI
{
	public class ThinkNode_ChancePerHour_Lovin : ThinkNode_ChancePerHour
	{
		protected override float MtbHours(Pawn pawn)
		{
			if (pawn.CurJob == null || !pawn.jobs.curDriver.layingDown || pawn.jobs.curDriver.layingDownBed == null)
			{
				return -1f;
			}
			Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
			if (partnerInMyBed == null)
			{
				return -1f;
			}
			return LovePartnerRelationUtility.GetLovinMtbHours(pawn, partnerInMyBed);
		}
	}
}
