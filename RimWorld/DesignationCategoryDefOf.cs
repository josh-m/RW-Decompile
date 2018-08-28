using System;
using Verse;

namespace RimWorld
{
	[DefOf]
	public static class DesignationCategoryDefOf
	{
		public static DesignationCategoryDef Production;

		public static DesignationCategoryDef Structure;

		public static DesignationCategoryDef Security;

		static DesignationCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DesignationCategoryDefOf));
		}
	}
}
