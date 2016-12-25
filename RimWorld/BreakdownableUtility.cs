using System;
using Verse;

namespace RimWorld
{
	public static class BreakdownableUtility
	{
		public static bool IsBrokenDown(this Thing t)
		{
			CompBreakdownable compBreakdownable = t.TryGetComp<CompBreakdownable>();
			return compBreakdownable != null && compBreakdownable.BrokenDown;
		}
	}
}
