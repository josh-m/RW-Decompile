using System;

namespace RimWorld
{
	[DefOf]
	public static class ScenarioDefOf
	{
		public static ScenarioDef Crashlanded;

		static ScenarioDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ScenarioDefOf));
		}
	}
}
