using System;
using System.Collections.Generic;

namespace Verse.Grammar
{
	public struct GrammarRequest
	{
		private List<Rule> rules;

		private List<RulePackDef> includes;

		private Dictionary<string, string> constants;

		public List<Rule> Rules
		{
			get
			{
				if (this.rules == null)
				{
					this.rules = new List<Rule>();
				}
				return this.rules;
			}
		}

		public List<RulePackDef> Includes
		{
			get
			{
				if (this.includes == null)
				{
					this.includes = new List<RulePackDef>();
				}
				return this.includes;
			}
		}

		public Dictionary<string, string> Constants
		{
			get
			{
				if (this.constants == null)
				{
					this.constants = new Dictionary<string, string>();
				}
				return this.constants;
			}
		}

		public void Clear()
		{
			if (this.rules != null)
			{
				this.rules.Clear();
			}
			if (this.includes != null)
			{
				this.includes.Clear();
			}
			if (this.constants != null)
			{
				this.constants.Clear();
			}
		}

		public List<Rule> GetRules()
		{
			return this.rules;
		}

		public List<RulePackDef> GetIncludes()
		{
			return this.includes;
		}

		public Dictionary<string, string> GetConstants()
		{
			return this.constants;
		}
	}
}
