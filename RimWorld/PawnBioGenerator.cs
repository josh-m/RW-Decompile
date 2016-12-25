using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnBioGenerator
	{
		private const float SolidBioChance = 0.25f;

		private const float PreferredNameChance = 0.4f;

		public static void GiveAppropriateBioTo(Pawn pawn, string forcedLastName)
		{
			if (Rand.Value < 0.25f || pawn.kindDef.factionLeader)
			{
				if (!PawnBioGenerator.TryGiveSolidBioTo(pawn, forcedLastName))
				{
					PawnBioGenerator.GiveShuffledBioTo(pawn, pawn.Faction.def, forcedLastName);
				}
			}
			else
			{
				PawnBioGenerator.GiveShuffledBioTo(pawn, pawn.Faction.def, forcedLastName);
			}
		}

		private static void GiveShuffledBioTo(Pawn pawn, FactionDef factionType, string forcedLastName)
		{
			pawn.Name = NameGenerator.GeneratePawnName(pawn, NameStyle.Full, forcedLastName);
			PawnBioGenerator.SetBackstoryInSlot(pawn, BackstorySlot.Childhood, ref pawn.story.childhood, factionType);
			PawnBioGenerator.SetBackstoryInSlot(pawn, BackstorySlot.Adulthood, ref pawn.story.adulthood, factionType);
		}

		private static void SetBackstoryInSlot(Pawn pawn, BackstorySlot slot, ref Backstory backstory, FactionDef factionType)
		{
			if (!(from kvp in BackstoryDatabase.allBackstories
			where kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(factionType.backstoryCategory) && kvp.Value.slot == slot
			select kvp.Value).TryRandomElement(out backstory))
			{
				Log.Error(string.Concat(new object[]
				{
					"No shuffled ",
					slot,
					" found for ",
					pawn,
					" of ",
					factionType,
					". Defaulting."
				}));
				backstory = (from kvp in BackstoryDatabase.allBackstories
				where kvp.Value.slot == slot
				select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
			}
		}

		private static bool TryGiveSolidBioTo(Pawn pawn, string forcedLastName)
		{
			string backstoryCategory = pawn.Faction.def.backstoryCategory;
			PawnBio pawnBio = PawnBioGenerator.TryGetRandomUnusedSolidBioFor(backstoryCategory, pawn.kindDef.factionLeader, pawn.gender, forcedLastName);
			if (pawnBio == null)
			{
				return false;
			}
			if (pawnBio.name.First == "Tynan" && pawnBio.name.Last == "Sylvester" && Rand.Value < 0.5f)
			{
				pawnBio = PawnBioGenerator.TryGetRandomUnusedSolidBioFor(backstoryCategory, pawn.kindDef.factionLeader, pawn.gender, forcedLastName);
			}
			if (pawnBio == null)
			{
				return false;
			}
			pawn.Name = pawnBio.name;
			pawn.story.childhood = pawnBio.childhood;
			pawn.story.adulthood = pawnBio.adulthood;
			return true;
		}

		private static PawnBio TryGetRandomUnusedSolidBioFor(string backstoryCategory, bool factionLeader, Gender gender, string forcedLastName)
		{
			IEnumerable<PawnBio> source = SolidBioDatabase.allBios.Where(delegate(PawnBio bio)
			{
				if (bio.pirateKing != factionLeader)
				{
					return false;
				}
				if (bio.gender != GenderPossibility.Either)
				{
					if (gender == Gender.Male && bio.gender != GenderPossibility.Male)
					{
						return false;
					}
					if (gender == Gender.Female && bio.gender != GenderPossibility.Female)
					{
						return false;
					}
				}
				if (!forcedLastName.NullOrEmpty() && bio.name.Last != forcedLastName)
				{
					return false;
				}
				if (bio.name.UsedThisGame)
				{
					return false;
				}
				for (int i = 0; i < bio.adulthood.spawnCategories.Count; i++)
				{
					if (bio.adulthood.spawnCategories[i] == backstoryCategory)
					{
						return true;
					}
				}
				return false;
			});
			if (Rand.Value < 0.4f)
			{
				string prefName = Prefs.RandomPreferredName;
				if (!prefName.NullOrEmpty())
				{
					source = from bio in source
					where bio.name.Last == prefName
					select bio;
				}
			}
			PawnBio result;
			if (source.TryRandomElement(out result))
			{
				return result;
			}
			return null;
		}
	}
}
