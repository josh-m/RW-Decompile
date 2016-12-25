using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse.Grammar
{
	public static class GrammarResolver
	{
		private class RuleEntry
		{
			public Rule rule;

			public bool knownUnresolvable;

			public int uses;

			public float SelectionWeight
			{
				get
				{
					return this.rule.BaseSelectionWeight * 100000f / (float)((this.uses + 1) * 1000);
				}
			}

			public RuleEntry(Rule rule)
			{
				this.rule = rule;
				this.knownUnresolvable = false;
			}

			public void MarkKnownUnresolvable()
			{
				this.knownUnresolvable = true;
			}

			public override string ToString()
			{
				return this.rule.ToString();
			}
		}

		private const int DepthLimit = 50;

		private const int LoopsLimit = 100;

		private static List<GrammarResolver.RuleEntry> rules = new List<GrammarResolver.RuleEntry>();

		private static int loopCount;

		private static StringBuilder logSb;

		private static List<GrammarResolver.RuleEntry> matchingRules = new List<GrammarResolver.RuleEntry>();

		private static bool LogOn
		{
			get
			{
				return DebugViewSettings.logGrammarResolution;
			}
		}

		public static string Resolve(string rootKeyword, List<Rule> rawRules, string debugLabel = null)
		{
			GrammarResolver.rules.Clear();
			for (int i = 0; i < rawRules.Count; i++)
			{
				GrammarResolver.rules.Add(new GrammarResolver.RuleEntry(rawRules[i]));
			}
			for (int j = 0; j < RulePackDefOf.GlobalUtility.Rules.Count; j++)
			{
				GrammarResolver.rules.Add(new GrammarResolver.RuleEntry(RulePackDefOf.GlobalUtility.Rules[j]));
			}
			GrammarResolver.loopCount = 0;
			if (GrammarResolver.LogOn)
			{
				GrammarResolver.logSb = new StringBuilder();
			}
			string text = "err";
			bool flag = false;
			foreach (GrammarResolver.RuleEntry current in (from r in GrammarResolver.rules
			where r.rule.keyword == rootKeyword
			select r).InRandomOrder(null))
			{
				if (GrammarResolver.TryResolveRecursive(current, 0, out text))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				text = "Could not resolve any root: " + rootKeyword;
				if (!debugLabel.NullOrEmpty())
				{
					text = text + " debugLabel: " + debugLabel;
				}
				if (GrammarResolver.LogOn)
				{
					GrammarResolver.logSb.Insert(0, "FAILED TO RESOLVE\n");
				}
			}
			text = GenText.CapitalizeSentences(Find.ActiveLanguageWorker.PostProcessed(text));
			if (GrammarResolver.LogOn)
			{
				Log.Message(GrammarResolver.logSb.ToString().Trim());
			}
			return text;
		}

		private static bool TryResolveRecursive(GrammarResolver.RuleEntry entry, int depth, out string output)
		{
			if (GrammarResolver.LogOn)
			{
				GrammarResolver.logSb.AppendLine();
				GrammarResolver.logSb.Append(depth.ToStringCached() + " ");
				for (int i = 0; i < depth; i++)
				{
					GrammarResolver.logSb.Append("   ");
				}
				GrammarResolver.logSb.Append(entry + " ");
			}
			GrammarResolver.loopCount++;
			if (GrammarResolver.loopCount > 100)
			{
				Log.Error("Hit loops limit resolving grammar.");
				output = "HIT_LOOPS_LIMIT";
				if (GrammarResolver.LogOn)
				{
					GrammarResolver.logSb.Append("UNRESOLVABLE: Hit loops limit");
				}
				return false;
			}
			if (depth > 50)
			{
				Log.Error("Grammar recurred too deep while resolving keyword (>" + 50 + " deep)");
				output = "DEPTH_LIMIT_REACHED";
				if (GrammarResolver.LogOn)
				{
					GrammarResolver.logSb.Append("UNRESOLVABLE: Depth limit reached");
				}
				return false;
			}
			string text = entry.rule.Generate();
			bool flag = false;
			int num = -1;
			for (int j = 0; j < text.Length; j++)
			{
				char c = text[j];
				if (c == '[')
				{
					num = j;
				}
				if (c == ']')
				{
					if (num == -1)
					{
						Log.Error("Could not resolve rule " + text + ": mismatched brackets.");
						output = "MISMATCHED_BRACKETS";
						if (GrammarResolver.LogOn)
						{
							GrammarResolver.logSb.Append("UNRESOLVABLE: Mismatched brackets");
						}
						flag = true;
					}
					else
					{
						string text2 = text.Substring(num + 1, j - num - 1);
						string str;
						while (true)
						{
							GrammarResolver.RuleEntry ruleEntry = GrammarResolver.RandomPossiblyResolvableEntry(text2);
							if (ruleEntry == null)
							{
								break;
							}
							ruleEntry.uses++;
							if (GrammarResolver.TryResolveRecursive(ruleEntry, depth + 1, out str))
							{
								goto Block_13;
							}
							ruleEntry.MarkKnownUnresolvable();
						}
						entry.MarkKnownUnresolvable();
						output = "CANNOT_RESOLVE_SUBKEYWORD:" + text2;
						if (GrammarResolver.LogOn)
						{
							GrammarResolver.logSb.Append("UNRESOLVABLE: Cannot resolve sub-keyword '" + text2 + "'");
						}
						flag = true;
						goto IL_21E;
						Block_13:
						text = text.Substring(0, num) + str + text.Substring(j + 1);
						j = num;
					}
				}
				IL_21E:;
			}
			output = text;
			return !flag;
		}

		private static GrammarResolver.RuleEntry RandomPossiblyResolvableEntry(string keyword)
		{
			GrammarResolver.matchingRules.Clear();
			for (int i = 0; i < GrammarResolver.rules.Count; i++)
			{
				if (GrammarResolver.rules[i].rule.keyword == keyword && !GrammarResolver.rules[i].knownUnresolvable)
				{
					GrammarResolver.matchingRules.Add(GrammarResolver.rules[i]);
				}
			}
			if (GrammarResolver.matchingRules.Count == 0)
			{
				return null;
			}
			return GrammarResolver.matchingRules.RandomElementByWeight((GrammarResolver.RuleEntry r) => r.SelectionWeight);
		}
	}
}
