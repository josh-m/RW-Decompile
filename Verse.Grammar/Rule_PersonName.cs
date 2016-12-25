using RimWorld;
using System;

namespace Verse.Grammar
{
	public class Rule_PersonName : Rule
	{
		public int selectionWeight = 1;

		public Gender gender;

		public override float BaseSelectionWeight
		{
			get
			{
				return (float)this.selectionWeight;
			}
		}

		public override string Generate()
		{
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);
			Gender gender = this.gender;
			if (gender == Gender.None)
			{
				gender = ((Rand.Value >= 0.5f) ? Gender.Female : Gender.Male);
			}
			return nameBank.GetName(PawnNameSlot.First, gender);
		}

		public override string ToString()
		{
			return this.keyword + "->(personname)";
		}
	}
}
