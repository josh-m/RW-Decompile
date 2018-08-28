using System;

namespace RimWorld
{
	[DefOf]
	public static class MainButtonDefOf
	{
		public static MainButtonDef Inspect;

		public static MainButtonDef Architect;

		public static MainButtonDef Research;

		public static MainButtonDef Menu;

		public static MainButtonDef World;

		static MainButtonDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MainButtonDefOf));
		}
	}
}
