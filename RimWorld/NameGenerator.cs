using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public static class NameGenerator
	{
		private const float SolidNameChance = 0.5f;

		public const int MaxNameLength = 12;

		public const int MaxNickLength = 9;

		private const float NicknameChance = 0.15f;

		public static Name GeneratePawnName(Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
		{
			if (style == NameStyle.Full)
			{
				RulePackDef nameGenerator = pawn.RaceProps.GetNameGenerator(pawn.gender);
				if (nameGenerator != null)
				{
					string name = NameGenerator.GenerateName(nameGenerator, (string x) => !new NameSingle(x, false).UsedThisGame);
					return new NameSingle(name, false);
				}
				if (pawn.Faction != null && pawn.Faction.def.pawnNameMaker != null)
				{
					string rawName = NameGenerator.GenerateName(pawn.Faction.def.pawnNameMaker, delegate(string x)
					{
						NameTriple nameTriple4 = NameTriple.FromString(x);
						nameTriple4.ResolveMissingPieces(forcedLastName);
						return !nameTriple4.UsedThisGame;
					});
					NameTriple nameTriple = NameTriple.FromString(rawName);
					nameTriple.CapitalizeNick();
					nameTriple.ResolveMissingPieces(forcedLastName);
					return nameTriple;
				}
				if (pawn.RaceProps.nameCategory != PawnNameCategory.NoName)
				{
					NameTriple nameTriple2;
					if (Rand.Value < 0.5f)
					{
						nameTriple2 = PawnNameDatabaseSolid.RandomUnusedSolidName(pawn.gender, forcedLastName);
						if (nameTriple2 == null)
						{
							nameTriple2 = NameGenerator.GeneratePawnName_Shuffled(pawn, forcedLastName);
						}
					}
					else
					{
						nameTriple2 = NameGenerator.GeneratePawnName_Shuffled(pawn, forcedLastName);
					}
					return nameTriple2;
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
						if (!NameUseChecker.NameSingleIsUsedOnMap(text))
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
			string name = nameBank.GetName(pawn.gender, PawnNameSlot.First);
			string text;
			if (forcedLastName == null)
			{
				text = nameBank.GetName(Gender.None, PawnNameSlot.Last);
			}
			else
			{
				text = forcedLastName;
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
					nick = nameBank.GetName(gender, PawnNameSlot.Nick);
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

		public static string GenerateName(RulePackDef rootPack, IEnumerable<string> extantNames)
		{
			return NameGenerator.GenerateName(rootPack, (string x) => !extantNames.Contains(x));
		}

		public static string GenerateName(RulePackDef rootPack, Predicate<string> validator = null)
		{
			string text = null;
			for (int i = 0; i < 150; i++)
			{
				text = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(rootPack.Rules[0].keyword, rootPack.Rules, null));
				if (validator == null || validator(text))
				{
					return text;
				}
			}
			Log.Error("Could not get new name.");
			return text;
		}
	}
}
