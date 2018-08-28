using RimWorld;
using System;

namespace Verse.Grammar
{
	public class Rule_NamePerson : Rule
	{
		public Gender gender;

		public override float BaseSelectionWeight
		{
			get
			{
				return 1f;
			}
		}

		public override Rule DeepCopy()
		{
			Rule_NamePerson rule_NamePerson = (Rule_NamePerson)base.DeepCopy();
			rule_NamePerson.gender = this.gender;
			return rule_NamePerson;
		}

		public override string Generate()
		{
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);
			Gender gender = this.gender;
			if (gender == Gender.None)
			{
				gender = ((Rand.Value >= 0.5f) ? Gender.Female : Gender.Male);
			}
			return nameBank.GetName(PawnNameSlot.First, gender, false);
		}

		public override string ToString()
		{
			return this.keyword + "->(personname)";
		}
	}
}
