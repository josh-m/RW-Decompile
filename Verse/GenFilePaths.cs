using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenFilePaths
	{
		public const string SoundsFolder = "Sounds/";

		public const string TexturesFolder = "Textures/";

		public const string StringsFolder = "Strings/";

		public const string DefsFolder = "Defs/";

		public const string BackstoriesPath = "Backstories";

		public const string SavedGameExtension = ".rws";

		public const string ScenarioExtension = ".rsc";

		public const string ExternalHistoryFileExtension = ".rwh";

		private const string SaveDataFolderCommand = "savedatafolder";

		private static string saveDataPath = null;

		private static string coreModsFolderPath = null;

		private static readonly string[] FilePathRaw = new string[]
		{
			"Ž",
			"ž",
			"Ÿ",
			"¡",
			"¢",
			"£",
			"¤",
			"¥",
			"¦",
			"§",
			"¨",
			"©",
			"ª",
			"À",
			"Á",
			"Â",
			"Ã",
			"Ä",
			"Å",
			"Æ",
			"Ç",
			"È",
			"É",
			"Ê",
			"Ë",
			"Ì",
			"Í",
			"Î",
			"Ï",
			"Ð",
			"Ñ",
			"Ò",
			"Ó",
			"Ô",
			"Õ",
			"Ö",
			"Ù",
			"Ú",
			"Û",
			"Ü",
			"Ý",
			"Þ",
			"ß",
			"à",
			"á",
			"â",
			"ã",
			"ä",
			"å",
			"æ",
			"ç",
			"è",
			"é",
			"ê",
			"ë",
			"ì",
			"í",
			"î",
			"ï",
			"ð",
			"ñ",
			"ò",
			"ó",
			"ô",
			"õ",
			"ö",
			"ù",
			"ú",
			"û",
			"ü",
			"ý",
			"þ",
			"ÿ"
		};

		private static readonly string[] FilePathSafe = new string[]
		{
			"%8E",
			"%9E",
			"%9F",
			"%A1",
			"%A2",
			"%A3",
			"%A4",
			"%A5",
			"%A6",
			"%A7",
			"%A8",
			"%A9",
			"%AA",
			"%C0",
			"%C1",
			"%C2",
			"%C3",
			"%C4",
			"%C5",
			"%C6",
			"%C7",
			"%C8",
			"%C9",
			"%CA",
			"%CB",
			"%CC",
			"%CD",
			"%CE",
			"%CF",
			"%D0",
			"%D1",
			"%D2",
			"%D3",
			"%D4",
			"%D5",
			"%D6",
			"%D9",
			"%DA",
			"%DB",
			"%DC",
			"%DD",
			"%DE",
			"%DF",
			"%E0",
			"%E1",
			"%E2",
			"%E3",
			"%E4",
			"%E5",
			"%E6",
			"%E7",
			"%E8",
			"%E9",
			"%EA",
			"%EB",
			"%EC",
			"%ED",
			"%EE",
			"%EF",
			"%F0",
			"%F1",
			"%F2",
			"%F3",
			"%F4",
			"%F5",
			"%F6",
			"%F9",
			"%FA",
			"%FB",
			"%FC",
			"%FD",
			"%FE",
			"%FF"
		};

		public static string SaveDataFolderPath
		{
			get
			{
				if (GenFilePaths.saveDataPath == null)
				{
					string text;
					if (GenCommandLine.TryGetCommandLineArg("savedatafolder", out text))
					{
						text.TrimEnd(new char[]
						{
							'\\',
							'/'
						});
						if (text == string.Empty)
						{
							text = string.Empty + Path.DirectorySeparatorChar;
						}
						GenFilePaths.saveDataPath = text;
						Log.Message("Save data folder overridden to " + GenFilePaths.saveDataPath);
					}
					else
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(UnityData.dataPath);
						if (UnityData.isEditor)
						{
							GenFilePaths.saveDataPath = Path.Combine(directoryInfo.Parent.ToString(), "SaveData");
						}
						else if (UnityData.platform == RuntimePlatform.OSXPlayer || UnityData.platform == RuntimePlatform.OSXEditor || UnityData.platform == RuntimePlatform.OSXDashboardPlayer)
						{
							DirectoryInfo parent = Directory.GetParent(UnityData.persistentDataPath);
							string path = Path.Combine(parent.ToString(), "RimWorld");
							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}
							GenFilePaths.saveDataPath = path;
						}
						else
						{
							GenFilePaths.saveDataPath = Application.persistentDataPath;
						}
					}
					DirectoryInfo directoryInfo2 = new DirectoryInfo(GenFilePaths.saveDataPath);
					if (!directoryInfo2.Exists)
					{
						directoryInfo2.Create();
					}
				}
				return GenFilePaths.saveDataPath;
			}
		}

		public static string ScenarioPreviewImagePath
		{
			get
			{
				if (!UnityData.isEditor)
				{
					return Path.Combine(GenFilePaths.ExecutibleDir.FullName, "ScenarioPreview.jpg");
				}
				return Path.Combine(Path.Combine(Path.Combine(GenFilePaths.ExecutibleDir.FullName, "PlatformSpecific"), "All"), "ScenarioPreview.jpg");
			}
		}

		private static DirectoryInfo ExecutibleDir
		{
			get
			{
				return new DirectoryInfo(UnityData.dataPath).Parent;
			}
		}

		public static string CoreModsFolderPath
		{
			get
			{
				if (GenFilePaths.coreModsFolderPath == null)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(UnityData.dataPath);
					DirectoryInfo directoryInfo2;
					if (UnityData.isEditor)
					{
						directoryInfo2 = directoryInfo;
					}
					else
					{
						directoryInfo2 = directoryInfo.Parent;
					}
					GenFilePaths.coreModsFolderPath = Path.Combine(directoryInfo2.ToString(), "Mods");
					if (UnityData.isDebugBuild)
					{
						DirectoryInfo directoryInfo3 = new DirectoryInfo(GenFilePaths.coreModsFolderPath);
						if (!directoryInfo3.Exists)
						{
							GenFilePaths.coreModsFolderPath = Path.Combine(directoryInfo.Parent.Parent.ToString(), "RimWorld/Assets/Mods");
						}
					}
					DirectoryInfo directoryInfo4 = new DirectoryInfo(GenFilePaths.coreModsFolderPath);
					if (!directoryInfo4.Exists)
					{
						directoryInfo4.Create();
					}
				}
				return GenFilePaths.coreModsFolderPath;
			}
		}

		private static string ConfigFolderPath
		{
			get
			{
				return GenFilePaths.FolderUnderSaveData("Config");
			}
		}

		private static string SavedGamesFolderPath
		{
			get
			{
				return GenFilePaths.FolderUnderSaveData("Saves");
			}
		}

		private static string ScenariosFolderPath
		{
			get
			{
				return GenFilePaths.FolderUnderSaveData("Scenarios");
			}
		}

		private static string ExternalHistoryFolderPath
		{
			get
			{
				return GenFilePaths.FolderUnderSaveData("External");
			}
		}

		public static string ScreenshotFolderPath
		{
			get
			{
				return GenFilePaths.FolderUnderSaveData("Screenshots");
			}
		}

		public static string DevOutputFolderPath
		{
			get
			{
				return GenFilePaths.FolderUnderSaveData("DevOutput");
			}
		}

		public static string ModsConfigFilePath
		{
			get
			{
				return Path.Combine(GenFilePaths.ConfigFolderPath, "ModsConfig.xml");
			}
		}

		public static string ConceptKnowledgeFilePath
		{
			get
			{
				return Path.Combine(GenFilePaths.ConfigFolderPath, "Knowledge.xml");
			}
		}

		public static string PrefsFilePath
		{
			get
			{
				return Path.Combine(GenFilePaths.ConfigFolderPath, "Prefs.xml");
			}
		}

		public static string KeyPrefsFilePath
		{
			get
			{
				return Path.Combine(GenFilePaths.ConfigFolderPath, "KeyPrefs.xml");
			}
		}

		public static string LastPlayedVersionFilePath
		{
			get
			{
				return Path.Combine(GenFilePaths.ConfigFolderPath, "LastPlayedVersion.txt");
			}
		}

		public static string BackstoryOutputFilePath
		{
			get
			{
				return Path.Combine(GenFilePaths.DevOutputFolderPath, "Fresh_Backstories.xml");
			}
		}

		public static string TempFolderPath
		{
			get
			{
				return Application.temporaryCachePath;
			}
		}

		public static IEnumerable<FileInfo> AllSavedGameFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.SavedGamesFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
				where f.Extension == ".rws"
				orderby f.LastWriteTime descending
				select f;
			}
		}

		public static IEnumerable<FileInfo> AllCustomScenarioFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.ScenariosFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
				where f.Extension == ".rsc"
				orderby f.LastWriteTime descending
				select f;
			}
		}

		public static IEnumerable<FileInfo> AllExternalHistoryFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.ExternalHistoryFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
				where f.Extension == ".rwh"
				orderby f.LastWriteTime descending
				select f;
			}
		}

		private static string FolderUnderSaveData(string folderName)
		{
			string text = Path.Combine(GenFilePaths.SaveDataFolderPath, folderName);
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return text;
		}

		public static string FilePathForSavedGame(string gameName)
		{
			return Path.Combine(GenFilePaths.SavedGamesFolderPath, gameName + ".rws");
		}

		public static string AbsPathForScenario(string scenarioName)
		{
			return Path.Combine(GenFilePaths.ScenariosFolderPath, scenarioName + ".rsc");
		}

		public static string ContentPath<T>()
		{
			if (typeof(T) == typeof(AudioClip))
			{
				return "Sounds/";
			}
			if (typeof(T) == typeof(Texture2D))
			{
				return "Textures/";
			}
			if (typeof(T) == typeof(string))
			{
				return "Strings/";
			}
			throw new ArgumentException();
		}

		public static string FolderPathRelativeToDefsFolder(string fullFolderPath, ModContentPack mod)
		{
			fullFolderPath = Path.GetFullPath(fullFolderPath).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			string text = Path.GetFullPath(Path.Combine(mod.RootDir, "Defs/")).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			if (!fullFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				fullFolderPath += Path.DirectorySeparatorChar;
			}
			if (!text.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				text += Path.DirectorySeparatorChar;
			}
			if (!fullFolderPath.StartsWith(text))
			{
				Log.Error(string.Concat(new string[]
				{
					"Can't get relative path. Path \"",
					fullFolderPath,
					"\" does not start with \"",
					text,
					"\"."
				}));
				return null;
			}
			if (fullFolderPath == text)
			{
				return string.Empty;
			}
			string text2 = fullFolderPath.Substring(text.Length);
			while (text2.StartsWith("/") || text2.StartsWith("\\"))
			{
				if (text2.Length == 1)
				{
					return string.Empty;
				}
				text2 = text2.Substring(1);
			}
			return text2;
		}

		public static string SafeURIForUnityWWWFromPath(string rawPath)
		{
			string text = rawPath;
			for (int i = 0; i < GenFilePaths.FilePathRaw.Length; i++)
			{
				text = text.Replace(GenFilePaths.FilePathRaw[i], GenFilePaths.FilePathSafe[i]);
			}
			return "file:///" + text;
		}
	}
}
