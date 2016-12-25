using System;

namespace RimWorld
{
	internal static class BackstoryHardcodedData
	{
		public static void InjectHardcodedData(Backstory bs)
		{
			if (bs.Title == "Urbworld sex slave")
			{
				bs.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bs.Title == "Pop idol")
			{
				bs.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bs.Title == "Amateur botanist")
			{
				bs.AddForcedTrait(TraitDefOf.GreenThumb, 0);
			}
			if (bs.Title == "Mechanoid nerd")
			{
				bs.AddDisallowedTrait(TraitDefOf.Gay, 0);
			}
			if (bs.Title == "Mad scientist")
			{
				bs.AddForcedTrait(TraitDefOf.Psychopath, 0);
			}
			if (bs.Title == "Urbworld politican")
			{
				bs.AddForcedTrait(TraitDefOf.Greedy, 0);
			}
			if (bs.Title == "Criminal tinker")
			{
				bs.AddForcedTrait(TraitDefOf.Bloodlust, 0);
			}
			if (bs.Title == "Urbworld enforcer")
			{
				bs.AddForcedTrait(TraitDefOf.Nerves, 1);
			}
			if (bs.Title == "Pyro assistant")
			{
				bs.AddForcedTrait(TraitDefOf.Pyromaniac, 0);
			}
			if (bs.Title == "Stiletto assassin")
			{
				bs.AddForcedTrait(TraitDefOf.Psychopath, 0);
			}
			if (bs.Title == "Discharged soldier")
			{
				bs.AddForcedTrait(TraitDefOf.TooSmart, 0);
			}
		}
	}
}
