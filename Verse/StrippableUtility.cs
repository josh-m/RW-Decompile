using System;

namespace Verse
{
	public static class StrippableUtility
	{
		public static bool CanBeStrippedByColony(Thing th)
		{
			IStrippable strippable = th as IStrippable;
			if (strippable == null)
			{
				return false;
			}
			if (!strippable.AnythingToStrip())
			{
				return false;
			}
			Pawn pawn = th as Pawn;
			return pawn == null || pawn.Downed || (pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure);
		}
	}
}
