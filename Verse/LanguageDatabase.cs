using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public static class LanguageDatabase
	{
		private static List<LoadedLanguage> languages = new List<LoadedLanguage>();

		public static LoadedLanguage activeLanguage;

		public static LoadedLanguage defaultLanguage;

		public static readonly string DefaultLangFolderName = "English";

		private static readonly List<string> SupportedAutoSelectLanguages = new List<string>
		{
			"Arabic",
			"ChineseSimplified",
			"ChineseTraditional",
			"Czech",
			"Danish",
			"Dutch",
			"English",
			"Estonian",
			"Finnish",
			"French",
			"German",
			"Hungarian",
			"Italian",
			"Japanese",
			"Korean",
			"Norwegian",
			"Polish",
			"Portuguese",
			"PortugueseBrazilian",
			"Romanian",
			"Russian",
			"Slovak",
			"Spanish",
			"SpanishLatin",
			"Swedish",
			"Turkish",
			"Ukrainian"
		};

		public static IEnumerable<LoadedLanguage> AllLoadedLanguages
		{
			get
			{
				return LanguageDatabase.languages;
			}
		}

		public static void SelectLanguage(LoadedLanguage lang)
		{
			Prefs.LangFolderName = lang.folderName;
			LongEventHandler.QueueLongEvent(delegate
			{
				PlayDataLoader.ClearAllPlayData();
				PlayDataLoader.LoadAllPlayData(false);
			}, "LoadingLongEvent", true, null);
		}

		public static void Clear()
		{
			LanguageDatabase.languages.Clear();
			LanguageDatabase.activeLanguage = null;
		}

		public static void LoadAllMetadata()
		{
			foreach (ModContentPack current in LoadedModManager.RunningMods)
			{
				string path = Path.Combine(current.RootDir, "Languages");
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				if (directoryInfo.Exists)
				{
					DirectoryInfo[] directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
					for (int i = 0; i < directories.Length; i++)
					{
						DirectoryInfo langDir = directories[i];
						LanguageDatabase.LoadLanguageMetadataFrom(langDir);
					}
				}
			}
			LanguageDatabase.defaultLanguage = LanguageDatabase.languages.FirstOrDefault((LoadedLanguage la) => la.folderName == LanguageDatabase.DefaultLangFolderName);
			LanguageDatabase.activeLanguage = LanguageDatabase.languages.FirstOrDefault((LoadedLanguage la) => la.folderName == Prefs.LangFolderName);
			if (LanguageDatabase.activeLanguage == null)
			{
				Prefs.LangFolderName = LanguageDatabase.DefaultLangFolderName;
				LanguageDatabase.activeLanguage = LanguageDatabase.languages.FirstOrDefault((LoadedLanguage la) => la.folderName == Prefs.LangFolderName);
			}
			if (LanguageDatabase.activeLanguage == null || LanguageDatabase.defaultLanguage == null)
			{
				Log.Error("No default language found!");
				LanguageDatabase.defaultLanguage = LanguageDatabase.languages[0];
				LanguageDatabase.activeLanguage = LanguageDatabase.languages[0];
			}
		}

		private static LoadedLanguage LoadLanguageMetadataFrom(DirectoryInfo langDir)
		{
			LoadedLanguage loadedLanguage = LanguageDatabase.languages.FirstOrDefault((LoadedLanguage lib) => lib.folderName == langDir.Name);
			if (loadedLanguage == null)
			{
				loadedLanguage = new LoadedLanguage(langDir.ToString());
				LanguageDatabase.languages.Add(loadedLanguage);
			}
			if (loadedLanguage != null)
			{
				loadedLanguage.TryLoadMetadataFrom(langDir.ToString());
			}
			return loadedLanguage;
		}

		public static string SystemLanguageFolderName()
		{
			if (SteamManager.Initialized)
			{
				string text = SteamApps.GetCurrentGameLanguage().CapitalizeFirst();
				if (LanguageDatabase.SupportedAutoSelectLanguages.Contains(text))
				{
					return text;
				}
			}
			string text2 = Application.systemLanguage.ToString();
			if (LanguageDatabase.SupportedAutoSelectLanguages.Contains(text2))
			{
				return text2;
			}
			return LanguageDatabase.DefaultLangFolderName;
		}
	}
}
