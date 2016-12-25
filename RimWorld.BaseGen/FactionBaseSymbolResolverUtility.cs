using System;

namespace RimWorld.BaseGen
{
	public static class FactionBaseSymbolResolverUtility
	{
		public static bool ShouldUseSandbags(Faction faction)
		{
			return !faction.def.techLevel.IsNeolithicOrWorse();
		}
	}
}
