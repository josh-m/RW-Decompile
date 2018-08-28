using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
	public class RulePack
	{
		[MustTranslate, TranslationCanChangeCount]
		private List<string> rulesStrings = new List<string>();

		[MayTranslate, TranslationCanChangeCount]
		private List<string> rulesFiles = new List<string>();

		private List<Rule> rulesRaw;

		[Unsaved]
		private List<Rule> rulesResolved;

		[Unsaved]
		private List<Rule> untranslatedRulesResolved;

		[Unsaved]
		private List<string> untranslatedRulesStrings;

		[Unsaved]
		private List<string> untranslatedRulesFiles;

		[Unsaved]
		private List<Rule> untranslatedRulesRaw;

		public List<Rule> Rules
		{
			get
			{
				if (this.rulesResolved == null)
				{
					this.rulesResolved = RulePack.GetRulesResolved(this.rulesRaw, this.rulesStrings, this.rulesFiles);
				}
				return this.rulesResolved;
			}
		}

		public List<Rule> UntranslatedRules
		{
			get
			{
				if (this.untranslatedRulesResolved == null)
				{
					this.untranslatedRulesResolved = RulePack.GetRulesResolved(this.untranslatedRulesRaw, this.untranslatedRulesStrings, this.untranslatedRulesFiles);
				}
				return this.untranslatedRulesResolved;
			}
		}

		public void PostLoad()
		{
			this.untranslatedRulesStrings = this.rulesStrings.ToList<string>();
			this.untranslatedRulesFiles = this.rulesFiles.ToList<string>();
			if (this.rulesRaw != null)
			{
				this.untranslatedRulesRaw = new List<Rule>();
				for (int i = 0; i < this.rulesRaw.Count; i++)
				{
					this.untranslatedRulesRaw.Add(this.rulesRaw[i].DeepCopy());
				}
			}
		}

		private static List<Rule> GetRulesResolved(List<Rule> rulesRaw, List<string> rulesStrings, List<string> rulesFiles)
		{
			List<Rule> list = new List<Rule>();
			for (int i = 0; i < rulesStrings.Count; i++)
			{
				try
				{
					Rule_String rule_String = new Rule_String(rulesStrings[i]);
					rule_String.Init();
					list.Add(rule_String);
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception parsing grammar rule from ",
						rulesStrings[i],
						": ",
						ex
					}), false);
				}
			}
			for (int j = 0; j < rulesFiles.Count; j++)
			{
				try
				{
					string[] array = rulesFiles[j].Split(new string[]
					{
						"->"
					}, StringSplitOptions.None);
					Rule_File rule_File = new Rule_File();
					rule_File.keyword = array[0].Trim();
					rule_File.path = array[1].Trim();
					rule_File.Init();
					list.Add(rule_File);
				}
				catch (Exception ex2)
				{
					Log.Error(string.Concat(new object[]
					{
						"Error initializing Rule_File ",
						rulesFiles[j],
						": ",
						ex2
					}), false);
				}
			}
			if (rulesRaw != null)
			{
				for (int k = 0; k < rulesRaw.Count; k++)
				{
					try
					{
						rulesRaw[k].Init();
						list.Add(rulesRaw[k]);
					}
					catch (Exception ex3)
					{
						Log.Error(string.Concat(new object[]
						{
							"Error initializing rule ",
							rulesRaw[k].ToStringSafe<Rule>(),
							": ",
							ex3
						}), false);
					}
				}
			}
			return list;
		}
	}
}
