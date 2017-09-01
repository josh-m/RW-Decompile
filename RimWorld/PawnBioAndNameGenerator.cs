using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnBioAndNameGenerator
	{
		private const float MinAgeForAdulthood = 20f;

		private const float SolidBioChance = 0.25f;

		private const float SolidNameChance = 0.5f;

		private const float TryPreferredNameChance_Bio = 0.5f;

		private const float TryPreferredNameChance_Name = 0.5f;

		private const float ShuffledNicknameChance = 0.15f;

		public static void GiveAppropriateBioAndNameTo(Pawn pawn, string requiredLastName)
		{
			if ((Rand.Value < 0.25f || pawn.kindDef.factionLeader) && PawnBioAndNameGenerator.TryGiveSolidBioTo(pawn, requiredLastName))
			{
				return;
			}
			PawnBioAndNameGenerator.GiveShuffledBioTo(pawn, pawn.Faction.def, requiredLastName);
		}

		private static void GiveShuffledBioTo(Pawn pawn, FactionDef factionType, string requiredLastName)
		{
			pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, requiredLastName);
			PawnBioAndNameGenerator.SetBackstoryInSlot(pawn, BackstorySlot.Childhood, ref pawn.story.childhood, factionType);
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
			{
				PawnBioAndNameGenerator.SetBackstoryInSlot(pawn, BackstorySlot.Adulthood, ref pawn.story.adulthood, factionType);
			}
		}

		private static void SetBackstoryInSlot(Pawn pawn, BackstorySlot slot, ref Backstory backstory, FactionDef factionType)
		{
			if (!(from kvp in BackstoryDatabase.allBackstories
			where kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(factionType.backstoryCategory) && kvp.Value.slot == slot && (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))
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

		private static bool TryGiveSolidBioTo(Pawn pawn, string requiredLastName)
		{
			PawnBio pawnBio = PawnBioAndNameGenerator.TryGetRandomUnusedSolidBioFor(pawn.Faction.def.backstoryCategory, pawn.kindDef, pawn.gender, requiredLastName);
			if (pawnBio == null)
			{
				return false;
			}
			if (pawnBio.name.First == "Tynan" && pawnBio.name.Last == "Sylvester" && Rand.Value < 0.5f)
			{
				pawnBio = PawnBioAndNameGenerator.TryGetRandomUnusedSolidBioFor(pawn.Faction.def.backstoryCategory, pawn.kindDef, pawn.gender, requiredLastName);
			}
			if (pawnBio == null)
			{
				return false;
			}
			pawn.Name = pawnBio.name;
			pawn.story.childhood = pawnBio.childhood;
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
			{
				pawn.story.adulthood = pawnBio.adulthood;
			}
			return true;
		}

		private static PawnBio TryGetRandomUnusedSolidBioFor(string backstoryCategory, PawnKindDef kind, Gender gender, string requiredLastName)
		{
			NameTriple prefName = null;
			if (Rand.Value < 0.5f)
			{
				prefName = Prefs.RandomPreferredName();
				if (prefName != null && (prefName.UsedThisGame || (requiredLastName != null && prefName.Last != requiredLastName)))
				{
					prefName = null;
				}
			}
			SolidBioDatabase.allBios.Shuffle<PawnBio>();
			PawnBio pawnBio;
			while (true)
			{
				pawnBio = SolidBioDatabase.allBios.FirstOrDefault(delegate(PawnBio bio)
				{
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
					return (requiredLastName.NullOrEmpty() || !(bio.name.Last != requiredLastName)) && (prefName == null || bio.name.Equals(prefName)) && (!kind.factionLeader || bio.pirateKing) && bio.adulthood.spawnCategories.Contains(backstoryCategory) && !bio.name.UsedThisGame;
				});
				if (pawnBio != null || prefName == null)
				{
					break;
				}
				prefName = null;
			}
			return pawnBio;
		}

		public static NameTriple TryGetRandomUnusedSolidName(Gender gender, string requiredLastName = null)
		{
			NameTriple nameTriple = null;
			if (Rand.Value < 0.5f)
			{
				nameTriple = Prefs.RandomPreferredName();
				if (nameTriple != null && (nameTriple.UsedThisGame || (requiredLastName != null && nameTriple.Last != requiredLastName)))
				{
					nameTriple = null;
				}
			}
			List<NameTriple> listForGender = PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
			List<NameTriple> list = (gender != Gender.Male) ? PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male);
			float num = ((float)listForGender.Count + 0.1f) / ((float)(listForGender.Count + list.Count) + 0.1f);
			List<NameTriple> list2;
			if (Rand.Value < num)
			{
				list2 = listForGender;
			}
			else
			{
				list2 = list;
			}
			if (list2.Count == 0)
			{
				Log.Error("Empty solid pawn name list for gender: " + gender + ".");
				return null;
			}
			if (nameTriple != null && list2.Contains(nameTriple))
			{
				return nameTriple;
			}
			list2.Shuffle<NameTriple>();
			return (from name in list2
			where (requiredLastName == null || !(name.Last != requiredLastName)) && !name.UsedThisGame
			select name).FirstOrDefault<NameTriple>();
		}

		public static Name GeneratePawnName(Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
		{
			if (style == NameStyle.Full)
			{
				RulePackDef nameGenerator = pawn.RaceProps.GetNameGenerator(pawn.gender);
				if (nameGenerator != null)
				{
					string name = NameGenerator.GenerateName(nameGenerator, (string x) => !new NameSingle(x, false).UsedThisGame, false);
					return new NameSingle(name, false);
				}
				if (pawn.Faction != null && pawn.Faction.def.pawnNameMaker != null)
				{
					string rawName = NameGenerator.GenerateName(pawn.Faction.def.pawnNameMaker, delegate(string x)
					{
						NameTriple nameTriple4 = NameTriple.FromString(x);
						nameTriple4.ResolveMissingPieces(forcedLastName);
						return !nameTriple4.UsedThisGame;
					}, false);
					NameTriple nameTriple = NameTriple.FromString(rawName);
					nameTriple.CapitalizeNick();
					nameTriple.ResolveMissingPieces(forcedLastName);
					return nameTriple;
				}
				if (pawn.RaceProps.nameCategory != PawnNameCategory.NoName)
				{
					if (Rand.Value < 0.5f)
					{
						NameTriple nameTriple2 = PawnBioAndNameGenerator.TryGetRandomUnusedSolidName(pawn.gender, forcedLastName);
						if (nameTriple2 != null)
						{
							return nameTriple2;
						}
					}
					return PawnBioAndNameGenerator.GeneratePawnName_Shuffled(pawn, forcedLastName);
				}
				Log.Error("No name making method for " + pawn);
				NameTriple nameTriple3 = NameTriple.FromString(pawn.def.label);
				nameTriple3.ResolveMissingPieces(null);
				return nameTriple3;
			}
			else
			{
				if (style == NameStyle.Numeric)
				{
					int num = 1;
					string text;
					while (true)
					{
						text = pawn.KindLabel + " " + num.ToString();
						if (!NameUseChecker.NameSingleIsUsed(text))
						{
							break;
						}
						num++;
					}
					return new NameSingle(text, true);
				}
				throw new InvalidOperationException();
			}
		}

		private static NameTriple GeneratePawnName_Shuffled(Pawn pawn, string forcedLastName = null)
		{
			PawnNameCategory pawnNameCategory = pawn.RaceProps.nameCategory;
			if (pawnNameCategory == PawnNameCategory.NoName)
			{
				Log.Message("Can't create a name of type NoName. Defaulting to HumanStandard.");
				pawnNameCategory = PawnNameCategory.HumanStandard;
			}
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(pawnNameCategory);
			string name = nameBank.GetName(PawnNameSlot.First, pawn.gender);
			string text;
			if (forcedLastName != null)
			{
				text = forcedLastName;
			}
			else
			{
				text = nameBank.GetName(PawnNameSlot.Last, Gender.None);
			}
			int num = 0;
			string nick;
			do
			{
				num++;
				if (Rand.Value < 0.15f)
				{
					Gender gender = pawn.gender;
					if (Rand.Value < 0.5f)
					{
						gender = Gender.None;
					}
					nick = nameBank.GetName(PawnNameSlot.Nick, gender);
				}
				else if (Rand.Value < 0.5f)
				{
					nick = name;
				}
				else
				{
					nick = text;
				}
			}
			while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any(delegate(Name x)
			{
				NameTriple nameTriple = x as NameTriple;
				return nameTriple != null && nameTriple.Nick == nick;
			}));
			return new NameTriple(name, nick, text);
		}
	}
}
