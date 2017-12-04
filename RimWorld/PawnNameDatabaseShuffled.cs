using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class PawnNameDatabaseShuffled
	{
		private static Dictionary<PawnNameCategory, NameBank> banks;

		static PawnNameDatabaseShuffled()
		{
			PawnNameDatabaseShuffled.banks = new Dictionary<PawnNameCategory, NameBank>();
			foreach (PawnNameCategory pawnNameCategory in Enum.GetValues(typeof(PawnNameCategory)))
			{
				if (pawnNameCategory != PawnNameCategory.NoName)
				{
					PawnNameDatabaseShuffled.banks.Add(pawnNameCategory, new NameBank(pawnNameCategory));
				}
			}
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);
			nameBank.AddNamesFromFile(PawnNameSlot.First, Gender.Male, "First_Male");
			nameBank.AddNamesFromFile(PawnNameSlot.First, Gender.Female, "First_Female");
			nameBank.AddNamesFromFile(PawnNameSlot.Nick, Gender.Male, "Nick_Male");
			nameBank.AddNamesFromFile(PawnNameSlot.Nick, Gender.Female, "Nick_Female");
			nameBank.AddNamesFromFile(PawnNameSlot.Nick, Gender.None, "Nick_Unisex");
			nameBank.AddNamesFromFile(PawnNameSlot.Last, Gender.None, "Last");
			foreach (NameBank current in PawnNameDatabaseShuffled.banks.Values)
			{
				current.ErrorCheck();
			}
		}

		public static NameBank BankOf(PawnNameCategory category)
		{
			return PawnNameDatabaseShuffled.banks[category];
		}
	}
}
