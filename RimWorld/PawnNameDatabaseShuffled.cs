using System;
using System.Collections;
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
			using (IEnumerator enumerator = Enum.GetValues(typeof(PawnNameCategory)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PawnNameCategory pawnNameCategory = (PawnNameCategory)((byte)enumerator.Current);
					if (pawnNameCategory != PawnNameCategory.NoName)
					{
						PawnNameDatabaseShuffled.banks.Add(pawnNameCategory, new NameBank(pawnNameCategory));
					}
				}
			}
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);
			nameBank.AddNamesFromFile(Gender.Male, PawnNameSlot.First, "First_Male");
			nameBank.AddNamesFromFile(Gender.Female, PawnNameSlot.First, "First_Female");
			nameBank.AddNamesFromFile(Gender.Male, PawnNameSlot.Nick, "Nick_Male");
			nameBank.AddNamesFromFile(Gender.Female, PawnNameSlot.Nick, "Nick_Female");
			nameBank.AddNamesFromFile(Gender.None, PawnNameSlot.Nick, "Nick_Unisex");
			nameBank.AddNamesFromFile(Gender.None, PawnNameSlot.Last, "Last");
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
