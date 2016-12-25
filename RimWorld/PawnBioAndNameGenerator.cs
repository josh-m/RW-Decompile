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

		private static List<PawnBio> tempBios = new List<PawnBio>();

		private static List<NameTriple> tempNames = new List<NameTriple>();

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
			NameTriple nameTriple = null;
			if (Rand.Value < 0.5f)
			{
				nameTriple = Prefs.RandomPreferredName();
				if (nameTriple != null && (nameTriple.UsedThisGame || (requiredLastName != null && nameTriple.Last != requiredLastName)))
				{
					nameTriple = null;
				}
			}
			while (true)
			{
				int i = 0;
				while (i < SolidBioDatabase.allBios.Count)
				{
					PawnBio pawnBio = SolidBioDatabase.allBios[i];
					if (pawnBio.gender == GenderPossibility.Either)
					{
						goto IL_8F;
					}
					if (gender != Gender.Male || pawnBio.gender == GenderPossibility.Male)
					{
						if (gender != Gender.Female || pawnBio.gender == GenderPossibility.Female)
						{
							goto IL_8F;
						}
					}
					IL_14E:
					i++;
					continue;
					IL_8F:
					if (!requiredLastName.NullOrEmpty() && pawnBio.name.Last != requiredLastName)
					{
						goto IL_14E;
					}
					if (pawnBio.name.UsedThisGame)
					{
						goto IL_14E;
					}
					if (nameTriple != null && !pawnBio.name.Equals(nameTriple))
					{
						goto IL_14E;
					}
					if (kind.factionLeader && !pawnBio.pirateKing)
					{
						goto IL_14E;
					}
					for (int j = 0; j < pawnBio.adulthood.spawnCategories.Count; j++)
					{
						if (pawnBio.adulthood.spawnCategories[j] == backstoryCategory)
						{
							PawnBioAndNameGenerator.tempBios.Add(pawnBio);
							break;
						}
					}
					goto IL_14E;
				}
				if (PawnBioAndNameGenerator.tempBios.Count != 0 || nameTriple == null)
				{
					break;
				}
				nameTriple = null;
			}
			PawnBio result;
			PawnBioAndNameGenerator.tempBios.TryRandomElement(out result);
			PawnBioAndNameGenerator.tempBios.Clear();
			return result;
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
			for (int i = 0; i < list2.Count; i++)
			{
				NameTriple nameTriple2 = list2[i];
				if (requiredLastName == null || !(nameTriple2.Last != requiredLastName))
				{
					if (!nameTriple2.UsedThisGame)
					{
						PawnBioAndNameGenerator.tempNames.Add(nameTriple2);
					}
				}
			}
			NameTriple result;
			PawnBioAndNameGenerator.tempNames.TryRandomElement(out result);
			PawnBioAndNameGenerator.tempNames.Clear();
			return result;
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
						if (!NameUseChecker.NameSingleIsUsedOnAnyMap(text))
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
