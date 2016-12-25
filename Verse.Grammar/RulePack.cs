using System;
using System.Collections.Generic;

namespace Verse.Grammar
{
	public class RulePack
	{
		private List<string> rulesStrings = new List<string>();

		private List<Rule> rulesRaw;

		[Unsaved]
		private List<Rule> rulesResolved;

		public List<Rule> Rules
		{
			get
			{
				if (this.rulesResolved == null)
				{
					this.rulesResolved = new List<Rule>();
					for (int i = 0; i < this.rulesStrings.Count; i++)
					{
						try
						{
							Rule_String item = new Rule_String(this.rulesStrings[i]);
							this.rulesResolved.Add(item);
						}
						catch (Exception ex)
						{
							Log.Error("Exception parsing grammar rule from " + this.rulesStrings[i] + ": " + ex.ToString());
						}
					}
					if (this.rulesRaw != null)
					{
						for (int j = 0; j < this.rulesRaw.Count; j++)
						{
							this.rulesRaw[j].Init();
							this.rulesResolved.Add(this.rulesRaw[j]);
						}
					}
				}
				return this.rulesResolved;
			}
		}
	}
}
