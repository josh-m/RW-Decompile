using System;

namespace RimWorld
{
	[DefOf]
	public static class WorkGiverDefOf
	{
		public static WorkGiverDef Refuel;

		static WorkGiverDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WorkGiverDefOf));
		}
	}
}
