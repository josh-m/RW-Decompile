using System;
using Verse;

namespace RimWorld
{
	public static class WardenFeedUtility
	{
		public static bool ShouldBeFed(Pawn p)
		{
			return p.IsPrisonerOfColony && p.InBed() && p.guest.ShouldBeBroughtFood && HealthAIUtility.ShouldSeekMedicalRest(p);
		}
	}
}
