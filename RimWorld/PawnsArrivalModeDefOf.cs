using System;

namespace RimWorld
{
	[DefOf]
	public static class PawnsArrivalModeDefOf
	{
		public static PawnsArrivalModeDef EdgeWalkIn;

		public static PawnsArrivalModeDef CenterDrop;

		public static PawnsArrivalModeDef EdgeDrop;

		static PawnsArrivalModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnsArrivalModeDefOf));
		}
	}
}
