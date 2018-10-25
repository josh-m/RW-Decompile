using System;

namespace RimWorld
{
	public static class BackstoryHardcodedData
	{
		public static void InjectHardcodedData(Backstory bs)
		{
			if (bs.title == "urbworld sex slave")
			{
				bs.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bs.title == "pop idol")
			{
				bs.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bs.title == "mechanoid nerd")
			{
				bs.AddDisallowedTrait(TraitDefOf.Gay, 0);
			}
			if (bs.title == "mad scientist")
			{
				bs.AddForcedTrait(TraitDefOf.Psychopath, 0);
			}
			if (bs.title == "urbworld politican")
			{
				bs.AddForcedTrait(TraitDefOf.Greedy, 0);
			}
			if (bs.title == "criminal tinker")
			{
				bs.AddForcedTrait(TraitDefOf.Bloodlust, 0);
			}
			if (bs.title == "urbworld enforcer")
			{
				bs.AddForcedTrait(TraitDefOf.Nerves, 1);
			}
			if (bs.title == "pyro assistant")
			{
				bs.AddForcedTrait(TraitDefOf.Pyromaniac, 0);
			}
			if (bs.title == "stiletto assassin")
			{
				bs.AddForcedTrait(TraitDefOf.Psychopath, 0);
			}
			if (bs.title == "discharged soldier")
			{
				bs.AddForcedTrait(TraitDefOf.TooSmart, 0);
			}
			if (bs.title == "bloody wanderer")
			{
				bs.AddForcedTrait(TraitDefOf.Bloodlust, 0);
			}
			if (bs.title == "new age duelist")
			{
				bs.AddForcedTrait(TraitDefOf.Industriousness, -1);
			}
			if (bs.title == "pirate doctor")
			{
				bs.AddForcedTrait(TraitDefOf.NaturalMood, 1);
			}
			if (bs.title == "cave child")
			{
				bs.AddForcedTrait(TraitDefOf.Undergrounder, 0);
			}
			if (bs.title == "space marine medic")
			{
				bs.AddForcedTrait(TraitDefOf.SpeedOffset, 2);
				bs.AddForcedTrait(TraitDefOf.ShootingAccuracy, -1);
			}
			if (bs.title == "bush sniper")
			{
				bs.AddForcedTrait(TraitDefOf.ShootingAccuracy, 1);
			}
		}

		public static void InjectHardcodedData(PawnBio bio)
		{
			if (bio.name.First == "Lia" && bio.name.Last == "Lu")
			{
				bio.childhood.AddForcedTrait(TraitDefOf.Beauty, 2);
			}
			if (bio.name.First == "Kena" && bio.name.Last == "Réveil")
			{
				bio.childhood.AddForcedTrait(TraitDefOf.PsychicSensitivity, 2);
				bio.childhood.AddForcedTrait(TraitDefOf.NaturalMood, -2);
			}
		}
	}
}
