using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Verse
{
	public static class Prefs
	{
		private static PrefsData data;

		public static float VolumeGame
		{
			get
			{
				return Prefs.data.volumeGame;
			}
			set
			{
				Prefs.data.volumeGame = value;
				Prefs.Apply();
			}
		}

		public static float VolumeMusic
		{
			get
			{
				return Prefs.data.volumeMusic;
			}
			set
			{
				Prefs.data.volumeMusic = value;
				Prefs.Apply();
			}
		}

		public static bool AdaptiveTrainingEnabled
		{
			get
			{
				return Prefs.data.adaptiveTrainingEnabled;
			}
			set
			{
				Prefs.data.adaptiveTrainingEnabled = value;
				Prefs.Apply();
			}
		}

		public static bool EdgeScreenScroll
		{
			get
			{
				return Prefs.data.edgeScreenScroll;
			}
			set
			{
				Prefs.data.edgeScreenScroll = value;
				Prefs.Apply();
			}
		}

		public static bool RunInBackground
		{
			get
			{
				return Prefs.data.runInBackground;
			}
			set
			{
				Prefs.data.runInBackground = value;
				Prefs.Apply();
			}
		}

		public static TemperatureDisplayMode TemperatureMode
		{
			get
			{
				return Prefs.data.temperatureMode;
			}
			set
			{
				Prefs.data.temperatureMode = value;
				Prefs.Apply();
			}
		}

		public static float AutosaveIntervalDays
		{
			get
			{
				return Prefs.data.autosaveIntervalDays;
			}
			set
			{
				Prefs.data.autosaveIntervalDays = value;
				Prefs.Apply();
			}
		}

		public static bool CustomCursorEnabled
		{
			get
			{
				return Prefs.data.customCursorEnabled;
			}
			set
			{
				Prefs.data.customCursorEnabled = value;
				Prefs.Apply();
			}
		}

		public static AnimalNameDisplayMode AnimalNameMode
		{
			get
			{
				return Prefs.data.animalNameMode;
			}
			set
			{
				Prefs.data.animalNameMode = value;
				Prefs.Apply();
			}
		}

		public static bool DevMode
		{
			get
			{
				return Prefs.data == null || Prefs.data.devMode;
			}
			set
			{
				Prefs.data.devMode = value;
				if (!Prefs.data.devMode)
				{
					Prefs.data.logVerbose = false;
					Prefs.data.resetModsConfigOnCrash = true;
				}
				Prefs.Apply();
			}
		}

		public static bool ResetModsConfigOnCrash
		{
			get
			{
				return Prefs.data == null || Prefs.data.resetModsConfigOnCrash;
			}
			set
			{
				Prefs.data.resetModsConfigOnCrash = value;
				Prefs.Apply();
			}
		}

		public static List<string> PreferredNames
		{
			get
			{
				return Prefs.data.preferredNames;
			}
			set
			{
				Prefs.data.preferredNames = value;
				Prefs.Apply();
			}
		}

		public static string LangFolderName
		{
			get
			{
				return Prefs.data.langFolderName;
			}
			set
			{
				Prefs.data.langFolderName = value;
				Prefs.Apply();
			}
		}

		public static bool LogVerbose
		{
			get
			{
				return Prefs.data.logVerbose;
			}
			set
			{
				Prefs.data.logVerbose = value;
				Prefs.Apply();
			}
		}

		public static bool PauseOnError
		{
			get
			{
				return Prefs.data != null && Prefs.data.pauseOnError;
			}
			set
			{
				Prefs.data.pauseOnError = value;
			}
		}

		public static bool PauseOnLoad
		{
			get
			{
				return Prefs.data.pauseOnLoad;
			}
			set
			{
				Prefs.data.pauseOnLoad = value;
			}
		}

		public static bool PauseOnUrgentLetter
		{
			get
			{
				return Prefs.data.pauseOnUrgentLetter;
			}
			set
			{
				Prefs.data.pauseOnUrgentLetter = value;
			}
		}

		public static bool ShowRealtimeClock
		{
			get
			{
				return Prefs.data.showRealtimeClock;
			}
			set
			{
				Prefs.data.showRealtimeClock = value;
			}
		}

		public static int MaxNumberOfPlayerHomes
		{
			get
			{
				return Prefs.data.maxNumberOfPlayerHomes;
			}
			set
			{
				Prefs.data.maxNumberOfPlayerHomes = value;
			}
		}

		public static bool PlantWindSway
		{
			get
			{
				return Prefs.data.plantWindSway;
			}
			set
			{
				Prefs.data.plantWindSway = value;
			}
		}

		public static bool ResourceReadoutCategorized
		{
			get
			{
				return Prefs.data.resourceReadoutCategorized;
			}
			set
			{
				if (value == Prefs.data.resourceReadoutCategorized)
				{
					return;
				}
				Prefs.data.resourceReadoutCategorized = value;
				Prefs.Save();
			}
		}

		public static float UIScale
		{
			get
			{
				return Prefs.data.uiScale;
			}
			set
			{
				Prefs.data.uiScale = value;
			}
		}

		public static void Init()
		{
			bool flag = !new FileInfo(GenFilePaths.PrefsFilePath).Exists;
			Prefs.data = new PrefsData();
			Prefs.data = XmlLoader.ItemFromXmlFile<PrefsData>(GenFilePaths.PrefsFilePath, true);
			if (flag)
			{
				Prefs.data.langFolderName = LanguageDatabase.SystemLanguageFolderName();
			}
		}

		public static void Save()
		{
			try
			{
				XDocument xDocument = new XDocument();
				XElement content = XmlSaver.XElementFromObject(Prefs.data, typeof(PrefsData));
				xDocument.Add(content);
				xDocument.Save(GenFilePaths.PrefsFilePath);
			}
			catch (Exception ex)
			{
				GenUI.ErrorDialog("ProblemSavingFile".Translate(new object[]
				{
					GenFilePaths.PrefsFilePath,
					ex.ToString()
				}));
				Log.Error("Exception saving prefs: " + ex);
			}
		}

		public static void Apply()
		{
			Prefs.data.Apply();
		}

		public static NameTriple RandomPreferredName()
		{
			string rawName;
			if ((from name in Prefs.PreferredNames
			where !name.NullOrEmpty()
			select name).TryRandomElement(out rawName))
			{
				return NameTriple.FromString(rawName);
			}
			return null;
		}
	}
}
