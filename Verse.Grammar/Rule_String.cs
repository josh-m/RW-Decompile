using System;

namespace Verse.Grammar
{
	public class Rule_String : Rule
	{
		private string output;

		public override float BaseSelectionWeight
		{
			get
			{
				return 1f;
			}
		}

		public Rule_String(string keyword, string output)
		{
			this.keyword = keyword;
			this.output = output;
		}

		public Rule_String(string rawString)
		{
			string[] array = rawString.Split(new string[]
			{
				"->"
			}, StringSplitOptions.None);
			this.keyword = array[0].Trim();
			this.output = array[1].Trim();
		}

		public override string Generate()
		{
			return this.output;
		}

		public override string ToString()
		{
			return this.keyword + "->" + this.output;
		}
	}
}
