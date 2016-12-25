using System;

namespace RimWorld
{
	internal static class BackstoryHardcodedData
	{
		public static void InjectHardcodedData(Backstory bs)
		{
			if (bs.title == "Urbworld sex slave")
			{
				bs.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bs.title == "Pop idol")
			{
				bs.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bs.title == "Amateur botanist")
			{
				bs.AddForcedTrait(TraitDefOf.GreenThumb, 0);
			}
			if (bs.title == "Mechanoid nerd")
			{
				bs.AddDisallowedTrait(TraitDefOf.Gay, 0);
			}
		}
	}
}
