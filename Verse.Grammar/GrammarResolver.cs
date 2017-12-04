using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse.Grammar
{
	public static class GrammarResolver
	{
		private class RuleEntry
		{
			public Rule rule;

			public bool knownUnresolvable;

			public bool constantConstraintsChecked;

			public bool constantConstraintsValid;

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

			public bool ValidateConstantConstraints(Dictionary<string, string> constraints)
			{
				if (!this.constantConstraintsChecked)
				{
					this.constantConstraintsValid = true;
					if (this.rule.constantConstraints != null)
					{
						for (int i = 0; i < this.rule.constantConstraints.Count; i++)
						{
							Rule.ConstrantConstraint constrantConstraint = this.rule.constantConstraints[i];
							if (!(constrantConstraint.value == string.Empty) || constraints != null)
							{
								if (constraints == null)
								{
									this.constantConstraintsValid = false;
									break;
								}
								if (constraints.TryGetValue(constrantConstraint.key) == constrantConstraint.value != constrantConstraint.equality)
								{
									this.constantConstraintsValid = false;
									break;
								}
							}
						}
					}
					this.constantConstraintsChecked = true;
				}
				return this.constantConstraintsValid;
			}

			public override string ToString()
			{
				return this.rule.ToString();
			}
		}

		private static SimpleLinearPool<List<GrammarResolver.RuleEntry>> rulePool = new SimpleLinearPool<List<GrammarResolver.RuleEntry>>();

		private static Dictionary<string, List<GrammarResolver.RuleEntry>> rules = new Dictionary<string, List<GrammarResolver.RuleEntry>>();

		private static int loopCount;

		private static StringBuilder logSb;

		private const int DepthLimit = 50;

		private const int LoopsLimit = 1000;

		private static Regex Spaces = new Regex(" +([ ,.])");

		private static void AddRule(Rule rule)
		{
			List<GrammarResolver.RuleEntry> list = null;
			if (!GrammarResolver.rules.TryGetValue(rule.keyword, out list))
			{
				list = GrammarResolver.rulePool.Get();
				list.Clear();
				GrammarResolver.rules[rule.keyword] = list;
			}
			list.Add(new GrammarResolver.RuleEntry(rule));
		}

		public static string Resolve(string rootKeyword, GrammarRequest request, string debugLabel = null, bool forceLog = false)
		{
			bool flag = forceLog || DebugViewSettings.logGrammarResolution;
			GrammarResolver.rules.Clear();
			GrammarResolver.rulePool.Clear();
			if (flag)
			{
				GrammarResolver.logSb = new StringBuilder();
			}
			List<Rule> list = request.GetRules();
			if (list != null)
			{
				if (flag)
				{
					GrammarResolver.logSb.AppendLine("Custom rules:");
				}
				for (int i = 0; i < list.Count; i++)
				{
					GrammarResolver.AddRule(list[i]);
					if (flag)
					{
						GrammarResolver.logSb.AppendLine("  " + list[i].ToString());
					}
				}
				if (flag)
				{
					GrammarResolver.logSb.AppendLine();
				}
			}
			List<RulePackDef> includes = request.GetIncludes();
			if (includes != null)
			{
				HashSet<RulePackDef> hashSet = new HashSet<RulePackDef>();
				List<RulePackDef> list2 = new List<RulePackDef>(includes);
				if (flag)
				{
					GrammarResolver.logSb.AppendLine("Includes:");
				}
				while (list2.Count > 0)
				{
					RulePackDef rulePackDef = list2[list2.Count - 1];
					list2.RemoveLast<RulePackDef>();
					if (!hashSet.Contains(rulePackDef))
					{
						if (flag)
						{
							GrammarResolver.logSb.AppendLine(string.Format("  {0}", rulePackDef.defName));
						}
						hashSet.Add(rulePackDef);
						List<Rule> rulesImmediate = rulePackDef.RulesImmediate;
						if (rulesImmediate != null)
						{
							foreach (Rule current in rulePackDef.RulesImmediate)
							{
								GrammarResolver.AddRule(current);
							}
						}
						if (!rulePackDef.include.NullOrEmpty<RulePackDef>())
						{
							list2.AddRange(rulePackDef.include);
						}
					}
				}
				if (flag)
				{
					GrammarResolver.logSb.AppendLine();
				}
			}
			for (int j = 0; j < RulePackDefOf.GlobalUtility.RulesPlusIncludes.Count; j++)
			{
				GrammarResolver.AddRule(RulePackDefOf.GlobalUtility.RulesPlusIncludes[j]);
			}
			GrammarResolver.loopCount = 0;
			Dictionary<string, string> constants = request.GetConstants();
			if (flag && constants != null)
			{
				GrammarResolver.logSb.AppendLine("Constants:");
				foreach (KeyValuePair<string, string> current2 in constants)
				{
					GrammarResolver.logSb.AppendLine(string.Format("  {0}: {1}", current2.Key, current2.Value));
				}
			}
			string text = "err";
			bool flag2 = false;
			if (!GrammarResolver.TryResolveRecursive(new GrammarResolver.RuleEntry(new Rule_String(string.Empty, "[" + rootKeyword + "]")), 0, constants, out text, flag))
			{
				flag2 = true;
				text = "Could not resolve any root: " + rootKeyword;
				if (!debugLabel.NullOrEmpty())
				{
					text = text + " debugLabel: " + debugLabel;
				}
				if (flag)
				{
					GrammarResolver.logSb.Insert(0, "FAILED TO RESOLVE\n");
				}
				else
				{
					GrammarResolver.Resolve(rootKeyword, request, debugLabel, true);
				}
			}
			text = GenText.CapitalizeSentences(Find.ActiveLanguageWorker.PostProcessed(text));
			text = GrammarResolver.Spaces.Replace(text, (Match match) => match.Groups[1].Value);
			if (flag && flag2)
			{
				if (DebugViewSettings.logGrammarResolution)
				{
					Log.Error(GrammarResolver.logSb.ToString().Trim());
				}
				else
				{
					Log.ErrorOnce(GrammarResolver.logSb.ToString().Trim(), GrammarResolver.logSb.ToString().GetHashCode());
				}
			}
			else if (flag)
			{
				Log.Message(GrammarResolver.logSb.ToString().Trim());
			}
			return text;
		}

		private static bool TryResolveRecursive(GrammarResolver.RuleEntry entry, int depth, Dictionary<string, string> constants, out string output, bool log)
		{
			if (log)
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
			if (GrammarResolver.loopCount > 1000)
			{
				Log.Error("Hit loops limit resolving grammar.");
				output = "HIT_LOOPS_LIMIT";
				if (log)
				{
					GrammarResolver.logSb.Append("UNRESOLVABLE: Hit loops limit");
				}
				return false;
			}
			if (depth > 50)
			{
				Log.Error("Grammar recurred too deep while resolving keyword (>" + 50 + " deep)");
				output = "DEPTH_LIMIT_REACHED";
				if (log)
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
						if (log)
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
							GrammarResolver.RuleEntry ruleEntry = GrammarResolver.RandomPossiblyResolvableEntry(text2, constants);
							if (ruleEntry == null)
							{
								break;
							}
							ruleEntry.uses++;
							if (GrammarResolver.TryResolveRecursive(ruleEntry, depth + 1, constants, out str, log))
							{
								goto Block_13;
							}
							ruleEntry.MarkKnownUnresolvable();
						}
						entry.MarkKnownUnresolvable();
						output = "CANNOT_RESOLVE_SUBKEYWORD:" + text2;
						if (log)
						{
							GrammarResolver.logSb.Append("UNRESOLVABLE: Cannot resolve sub-keyword '" + text2 + "'");
						}
						flag = true;
						goto IL_216;
						Block_13:
						text = text.Substring(0, num) + str + text.Substring(j + 1);
						j = num;
					}
				}
				IL_216:;
			}
			output = text;
			return !flag;
		}

		private static GrammarResolver.RuleEntry RandomPossiblyResolvableEntry(string keyword, Dictionary<string, string> constants)
		{
			List<GrammarResolver.RuleEntry> list = GrammarResolver.rules.TryGetValue(keyword);
			if (list == null)
			{
				return null;
			}
			return list.RandomElementByWeightWithFallback(delegate(GrammarResolver.RuleEntry rule)
			{
				if (rule.knownUnresolvable || !rule.ValidateConstantConstraints(constants))
				{
					return 0f;
				}
				return rule.SelectionWeight;
			}, null);
		}
	}
}
