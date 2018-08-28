using System;

namespace Verse.Grammar
{
	public class Rule_Number : Rule
	{
		private IntRange range = IntRange.zero;

		public int selectionWeight = 1;

		public override float BaseSelectionWeight
		{
			get
			{
				return (float)this.selectionWeight;
			}
		}

		public override Rule DeepCopy()
		{
			Rule_Number rule_Number = (Rule_Number)base.DeepCopy();
			rule_Number.range = this.range;
			rule_Number.selectionWeight = this.selectionWeight;
			return rule_Number;
		}

		public override string Generate()
		{
			return this.range.RandomInRange.ToString();
		}

		public override string ToString()
		{
			return this.keyword + "->(number: " + this.range.ToString() + ")";
		}
	}
}
