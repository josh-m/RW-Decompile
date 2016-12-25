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
