using System;
using System.Collections.Generic;

namespace Verse.Grammar
{
	public class Rule_List : Rule
	{
		private List<string> strings = new List<string>();

		public override float BaseSelectionWeight
		{
			get
			{
				return (float)this.strings.Count;
			}
		}

		public override string Generate()
		{
			return this.strings.RandomElement<string>();
		}

		public override string ToString()
		{
			return this.keyword + "->(list: " + this.strings[0] + " etc)";
		}
	}
}
