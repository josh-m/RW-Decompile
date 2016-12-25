using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public static class LanguageReportGenerator
	{
		public static void OutputTranslationReport()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
			if (activeLanguage == defaultLanguage)
			{
				Messages.Message("Please activate a non-English language to scan.", MessageSound.RejectInput);
				return;
			}
			activeLanguage.LoadData();
			defaultLanguage.LoadData();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Translation report for " + activeLanguage);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("========== Missing keyed translations =========");
			foreach (KeyValuePair<string, string> current in defaultLanguage.keyedReplacements)
			{
				if (!activeLanguage.HaveTextForKey(current.Key))
				{
					stringBuilder.AppendLine(current.Key + " - '" + current.Value + "'");
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("========== Unnecessary keyed translations (will never be used) =========");
			foreach (KeyValuePair<string, string> current2 in activeLanguage.keyedReplacements)
			{
				if (!defaultLanguage.HaveTextForKey(current2.Key))
				{
					stringBuilder.AppendLine(current2.Key + " - '" + current2.Value + "'");
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("========== Def-injected translations missing =========");
			stringBuilder.AppendLine("Note: This does NOT return any kind of sub-fields. So if there's a list of strings, or a sub-member of the def with a string in it or something, they won't be reported here.");
			foreach (DefInjectionPackage current3 in activeLanguage.defInjections)
			{
				foreach (string current4 in current3.MissingInjections())
				{
					stringBuilder.AppendLine(current3.defType.Name + ": " + current4);
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("========== Backstory translations missing =========");
			foreach (string current5 in BackstoryTranslationUtility.MissingBackstoryTranslations(activeLanguage))
			{
				stringBuilder.AppendLine(current5);
			}
			Log.Message(stringBuilder.ToString());
			Messages.Message("Translation report about " + activeLanguage.ToString() + " written to console. Hit ` to see it.", MessageSound.Standard);
		}
	}
}
