using System;
using Verse;

namespace RimWorld
{
	[DefOf]
	public static class BodyDefOf
	{
		public static BodyDef Human;

		static BodyDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BodyDefOf));
		}
	}
}
