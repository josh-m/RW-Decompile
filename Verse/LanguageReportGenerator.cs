using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse
{
	public static class LanguageReportGenerator
	{
		private const string FileName = "TranslationReport.txt";

		public static void SaveTranslationReport()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage && !defaultLanguage.anyError)
			{
				Messages.Message("Please activate a non-English language to scan.", MessageTypeDefOf.RejectInput, false);
				return;
			}
			activeLanguage.LoadData();
			defaultLanguage.LoadData();
			LongEventHandler.QueueLongEvent(new Action(LanguageReportGenerator.DoSaveTranslationReport), "GeneratingTranslationReport", true, null);
		}

		private static void DoSaveTranslationReport()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Translation report for " + activeLanguage);
			if (activeLanguage.defInjections.Any((DefInjectionPackage x) => x.usedOldRepSyntax))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Consider using <Something.Field.Example.Etc>translation</Something.Field.Example.Etc> def-injection syntax instead of <rep>.");
			}
			try
			{
				LanguageReportGenerator.AppendGeneralLoadErrors(stringBuilder);
			}
			catch (Exception arg)
			{
				Log.Error("Error while generating translation report (general load errors): " + arg, false);
			}
			try
			{
				LanguageReportGenerator.AppendDefInjectionsLoadErros(stringBuilder);
			}
			catch (Exception arg2)
			{
				Log.Error("Error while generating translation report (def-injections load errors): " + arg2, false);
			}
			try
			{
				LanguageReportGenerator.AppendBackstoriesLoadErrors(stringBuilder);
			}
			catch (Exception arg3)
			{
				Log.Error("Error while generating translation report (backstories load errors): " + arg3, false);
			}
			try
			{
				LanguageReportGenerator.AppendMissingKeyedTranslations(stringBuilder);
			}
			catch (Exception arg4)
			{
				Log.Error("Error while generating translation report (missing keyed translations): " + arg4, false);
			}
			List<string> list = new List<string>();
			try
			{
				LanguageReportGenerator.AppendMissingDefInjections(stringBuilder, list);
			}
			catch (Exception arg5)
			{
				Log.Error("Error while generating translation report (missing def-injections): " + arg5, false);
			}
			try
			{
				LanguageReportGenerator.AppendMissingBackstories(stringBuilder);
			}
			catch (Exception arg6)
			{
				Log.Error("Error while generating translation report (missing backstories): " + arg6, false);
			}
			try
			{
				LanguageReportGenerator.AppendUnnecessaryDefInjections(stringBuilder, list);
			}
			catch (Exception arg7)
			{
				Log.Error("Error while generating translation report (unnecessary def-injections): " + arg7, false);
			}
			try
			{
				LanguageReportGenerator.AppendRenamedDefInjections(stringBuilder);
			}
			catch (Exception arg8)
			{
				Log.Error("Error while generating translation report (renamed def-injections): " + arg8, false);
			}
			try
			{
				LanguageReportGenerator.AppendArgumentCountMismatches(stringBuilder);
			}
			catch (Exception arg9)
			{
				Log.Error("Error while generating translation report (argument count mismatches): " + arg9, false);
			}
			try
			{
				LanguageReportGenerator.AppendUnnecessaryKeyedTranslations(stringBuilder);
			}
			catch (Exception arg10)
			{
				Log.Error("Error while generating translation report (unnecessary keyed translations): " + arg10, false);
			}
			try
			{
				LanguageReportGenerator.AppendKeyedTranslationsMatchingEnglish(stringBuilder);
			}
			catch (Exception arg11)
			{
				Log.Error("Error while generating translation report (keyed translations matching English): " + arg11, false);
			}
			try
			{
				LanguageReportGenerator.AppendBackstoriesMatchingEnglish(stringBuilder);
			}
			catch (Exception arg12)
			{
				Log.Error("Error while generating translation report (backstories matching English): " + arg12, false);
			}
			try
			{
				LanguageReportGenerator.AppendDefInjectionsSyntaxSuggestions(stringBuilder);
			}
			catch (Exception arg13)
			{
				Log.Error("Error while generating translation report (def-injections syntax suggestions): " + arg13, false);
			}
			string text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			if (text.NullOrEmpty())
			{
				text = GenFilePaths.SaveDataFolderPath;
			}
			text = Path.Combine(text, "TranslationReport.txt");
			File.WriteAllText(text, stringBuilder.ToString());
			Messages.Message("MessageTranslationReportSaved".Translate(new object[]
			{
				Path.GetFullPath(text)
			}), MessageTypeDefOf.TaskCompletion, false);
		}

		private static void AppendGeneralLoadErrors(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string current in activeLanguage.loadErrors)
			{
				num++;
				stringBuilder.AppendLine(current);
			}
			sb.AppendLine();
			sb.AppendLine("========== General load errors (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendDefInjectionsLoadErros(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage current in activeLanguage.defInjections)
			{
				foreach (string current2 in current.loadErrors)
				{
					num++;
					stringBuilder.AppendLine(current2);
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations load errors (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendBackstoriesLoadErrors(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string current in activeLanguage.backstoriesLoadErrors)
			{
				num++;
				stringBuilder.AppendLine(current);
			}
			sb.AppendLine();
			sb.AppendLine("========== Backstories load errors (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendMissingKeyedTranslations(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> current in defaultLanguage.keyedReplacements)
			{
				if (!activeLanguage.HaveTextForKey(current.Key, false))
				{
					string text = string.Concat(new string[]
					{
						current.Key,
						" '",
						current.Value.value.Replace("\n", "\\n"),
						"' (English file: ",
						defaultLanguage.GetKeySourceFileAndLine(current.Key),
						")"
					});
					if (activeLanguage.HaveTextForKey(current.Key, true))
					{
						text = text + " (placeholder exists in " + activeLanguage.GetKeySourceFileAndLine(current.Key) + ")";
					}
					num++;
					stringBuilder.AppendLine(text);
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Missing keyed translations (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendMissingDefInjections(StringBuilder sb, List<string> outUnnecessaryDefInjections)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage current in activeLanguage.defInjections)
			{
				foreach (string current2 in current.MissingInjections(outUnnecessaryDefInjections))
				{
					num++;
					stringBuilder.AppendLine(current.defType.Name + ": " + current2);
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations missing (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendMissingBackstories(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string current in BackstoryTranslationUtility.MissingBackstoryTranslations(activeLanguage))
			{
				num++;
				stringBuilder.AppendLine(current);
			}
			sb.AppendLine();
			sb.AppendLine("========== Backstory translations missing (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendUnnecessaryDefInjections(StringBuilder sb, List<string> unnecessaryDefInjections)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string current in unnecessaryDefInjections)
			{
				num++;
				stringBuilder.AppendLine(current);
			}
			sb.AppendLine();
			sb.AppendLine("========== Unnecessary def-injected translations (marked as NoTranslate) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendRenamedDefInjections(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage current in activeLanguage.defInjections)
			{
				foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> current2 in current.injections)
				{
					if (!(current2.Value.path == current2.Value.nonBackCompatiblePath))
					{
						string text = current2.Value.nonBackCompatiblePath.Split(new char[]
						{
							'.'
						})[0];
						string text2 = current2.Value.path.Split(new char[]
						{
							'.'
						})[0];
						if (text != text2)
						{
							stringBuilder.AppendLine(string.Concat(new string[]
							{
								"Def has been renamed: ",
								text,
								" -> ",
								text2,
								", translation ",
								current2.Value.nonBackCompatiblePath,
								" should be renamed as well."
							}));
						}
						else
						{
							stringBuilder.AppendLine("Translation " + current2.Value.nonBackCompatiblePath + " should be renamed to " + current2.Value.path);
						}
						num++;
					}
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations using old, renamed defs (fixed automatically but can break in the next RimWorld version) (" + num + ") =========");
			sb.Append(stringBuilder);
		}

		private static void AppendArgumentCountMismatches(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string current in defaultLanguage.keyedReplacements.Keys.Intersect(activeLanguage.keyedReplacements.Keys))
			{
				if (!activeLanguage.keyedReplacements[current].isPlaceholder)
				{
					int num2 = LanguageReportGenerator.CountParametersInString(defaultLanguage.keyedReplacements[current].value);
					int num3 = LanguageReportGenerator.CountParametersInString(activeLanguage.keyedReplacements[current].value);
					if (num2 != num3)
					{
						num++;
						stringBuilder.AppendLine(string.Format("{0} ({1})\n  - '{2}'\n  - '{3}'", new object[]
						{
							current,
							activeLanguage.GetKeySourceFileAndLine(current),
							defaultLanguage.keyedReplacements[current].value.Replace("\n", "\\n"),
							activeLanguage.keyedReplacements[current].value.Replace("\n", "\\n")
						}));
					}
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Argument count mismatches (may or may not be incorrect) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendUnnecessaryKeyedTranslations(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> current in activeLanguage.keyedReplacements)
			{
				if (!defaultLanguage.HaveTextForKey(current.Key, false))
				{
					num++;
					stringBuilder.AppendLine(string.Concat(new string[]
					{
						current.Key,
						" '",
						current.Value.value.Replace("\n", "\\n"),
						"' (",
						activeLanguage.GetKeySourceFileAndLine(current.Key),
						")"
					}));
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Unnecessary keyed translations (will never be used) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendKeyedTranslationsMatchingEnglish(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> current in activeLanguage.keyedReplacements)
			{
				if (!current.Value.isPlaceholder)
				{
					string b;
					if (defaultLanguage.TryGetTextFromKey(current.Key, out b) && current.Value.value == b)
					{
						num++;
						stringBuilder.AppendLine(string.Concat(new string[]
						{
							current.Key,
							" '",
							current.Value.value.Replace("\n", "\\n"),
							"' (",
							activeLanguage.GetKeySourceFileAndLine(current.Key),
							")"
						}));
					}
				}
			}
			sb.AppendLine();
			sb.AppendLine("========== Keyed translations matching English (maybe ok) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendBackstoriesMatchingEnglish(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (string current in BackstoryTranslationUtility.BackstoryTranslationsMatchingEnglish(activeLanguage))
			{
				num++;
				stringBuilder.AppendLine(current);
			}
			sb.AppendLine();
			sb.AppendLine("========== Backstory translations matching English (maybe ok) (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		private static void AppendDefInjectionsSyntaxSuggestions(StringBuilder sb)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (DefInjectionPackage current in activeLanguage.defInjections)
			{
				foreach (string current2 in current.loadSyntaxSuggestions)
				{
					num++;
					stringBuilder.AppendLine(current2);
				}
			}
			if (num == 0)
			{
				return;
			}
			sb.AppendLine();
			sb.AppendLine("========== Def-injected translations syntax suggestions (" + num + ") ==========");
			sb.Append(stringBuilder);
		}

		public static int CountParametersInString(string input)
		{
			MatchCollection matchCollection = Regex.Matches(input, "(?<!\\{)\\{([0-9]+).*?\\}(?!})");
			if (matchCollection.Count == 0)
			{
				return 0;
			}
			return matchCollection.Cast<Match>().Max((Match m) => int.Parse(m.Groups[1].Value)) + 1;
		}
	}
}
