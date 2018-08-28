using System;

namespace RimWorld
{
	[DefOf]
	public static class RaidStrategyDefOf
	{
		public static RaidStrategyDef ImmediateAttack;

		static RaidStrategyDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
		}
	}
}
