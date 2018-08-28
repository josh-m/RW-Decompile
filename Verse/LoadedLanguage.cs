using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class LoadedLanguage
	{
		public class KeyedReplacement
		{
			public string key;

			public string value;

			public string fileSource;

			public int fileSourceLine;

			public string fileSourceFullPath;

			public bool isPlaceholder;
		}

		public string folderName;

		public LanguageInfo info;

		private LanguageWorker workerInt;

		private bool dataIsLoaded;

		public List<string> loadErrors = new List<string>();

		public List<string> backstoriesLoadErrors = new List<string>();

		public bool anyKeyedReplacementsXmlParseError;

		public string lastKeyedReplacementsXmlParseErrorInFile;

		public bool anyDefInjectionsXmlParseError;

		public string lastDefInjectionsXmlParseErrorInFile;

		public bool anyError;

		public Texture2D icon = BaseContent.BadTex;

		public Dictionary<string, LoadedLanguage.KeyedReplacement> keyedReplacements = new Dictionary<string, LoadedLanguage.KeyedReplacement>();

		public List<DefInjectionPackage> defInjections = new List<DefInjectionPackage>();

		public Dictionary<string, List<string>> stringFiles = new Dictionary<string, List<string>>();

		public const string OldKeyedTranslationsFolderName = "CodeLinked";

		public const string KeyedTranslationsFolderName = "Keyed";

		public const string OldDefInjectionsFolderName = "DefLinked";

		public const string DefInjectionsFolderName = "DefInjected";

		public const string LanguagesFolderName = "Languages";

		public const string PlaceholderText = "TODO";

		public string FriendlyNameNative
		{
			get
			{
				if (this.info == null || this.info.friendlyNameNative.NullOrEmpty())
				{
					return this.folderName;
				}
				return this.info.friendlyNameNative;
			}
		}

		public string FriendlyNameEnglish
		{
			get
			{
				if (this.info == null || this.info.friendlyNameEnglish.NullOrEmpty())
				{
					return this.folderName;
				}
				return this.info.friendlyNameEnglish;
			}
		}

		public IEnumerable<string> FolderPaths
		{
			get
			{
				foreach (ModContentPack mod in LoadedModManager.RunningMods)
				{
					string langDirPath = Path.Combine(mod.RootDir, "Languages");
					string myDirPath = Path.Combine(langDirPath, this.folderName);
					DirectoryInfo myDir = new DirectoryInfo(myDirPath);
					if (myDir.Exists)
					{
						yield return myDirPath;
					}
				}
			}
		}

		public LanguageWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (LanguageWorker)Activator.CreateInstance(this.info.languageWorkerClass);
				}
				return this.workerInt;
			}
		}

		public LoadedLanguage(string folderPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			this.folderName = directoryInfo.Name;
		}

		public void TryLoadMetadataFrom(string folderPath)
		{
			if (this.info == null)
			{
				string filePath = Path.Combine(folderPath.ToString(), "LanguageInfo.xml");
				this.info = DirectXmlLoader.ItemFromXmlFile<LanguageInfo>(filePath, false);
				if (this.info.friendlyNameNative.NullOrEmpty())
				{
					FileInfo fileInfo = new FileInfo(Path.Combine(folderPath.ToString(), "FriendlyName.txt"));
					if (fileInfo.Exists)
					{
						this.info.friendlyNameNative = GenFile.TextFromRawFile(fileInfo.ToString());
					}
				}
				if (this.info.friendlyNameNative.NullOrEmpty())
				{
					this.info.friendlyNameNative = this.folderName;
				}
				if (this.info.friendlyNameEnglish.NullOrEmpty())
				{
					this.info.friendlyNameEnglish = this.folderName;
				}
			}
		}

		public void LoadData()
		{
			if (this.dataIsLoaded)
			{
				return;
			}
			this.dataIsLoaded = true;
			DeepProfiler.Start("Loading language data: " + this.folderName);
			foreach (string current in this.FolderPaths)
			{
				string localFolderPath = current;
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					if (this.icon == BaseContent.BadTex)
					{
						FileInfo fileInfo = new FileInfo(Path.Combine(localFolderPath.ToString(), "LangIcon.png"));
						if (fileInfo.Exists)
						{
							this.icon = ModContentLoader<Texture2D>.LoadItem(fileInfo.FullName, null).contentItem;
						}
					}
				});
				DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(current.ToString(), "CodeLinked"));
				if (directoryInfo.Exists)
				{
					this.loadErrors.Add("Translations aren't called CodeLinked any more. Please rename to Keyed: " + directoryInfo);
				}
				else
				{
					directoryInfo = new DirectoryInfo(Path.Combine(current.ToString(), "Keyed"));
				}
				if (directoryInfo.Exists)
				{
					FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
					for (int i = 0; i < files.Length; i++)
					{
						FileInfo file = files[i];
						this.LoadFromFile_Keyed(file);
					}
				}
				DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(current.ToString(), "DefLinked"));
				if (directoryInfo2.Exists)
				{
					this.loadErrors.Add("Translations aren't called DefLinked any more. Please rename to DefInjected: " + directoryInfo2);
				}
				else
				{
					directoryInfo2 = new DirectoryInfo(Path.Combine(current.ToString(), "DefInjected"));
				}
				if (directoryInfo2.Exists)
				{
					DirectoryInfo[] directories = directoryInfo2.GetDirectories("*", SearchOption.TopDirectoryOnly);
					for (int j = 0; j < directories.Length; j++)
					{
						DirectoryInfo directoryInfo3 = directories[j];
						string name = directoryInfo3.Name;
						Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(name);
						if (typeInAnyAssembly == null && name.Length > 3)
						{
							typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(name.Substring(0, name.Length - 1));
						}
						if (typeInAnyAssembly == null)
						{
							this.loadErrors.Add(string.Concat(new string[]
							{
								"Error loading language from ",
								current,
								": dir ",
								directoryInfo3.Name,
								" doesn't correspond to any def type. Skipping..."
							}));
						}
						else
						{
							FileInfo[] files2 = directoryInfo3.GetFiles("*.xml", SearchOption.AllDirectories);
							for (int k = 0; k < files2.Length; k++)
							{
								FileInfo file2 = files2[k];
								this.LoadFromFile_DefInject(file2, typeInAnyAssembly);
							}
						}
					}
				}
				this.EnsureAllDefTypesHaveDefInjectionPackage();
				DirectoryInfo directoryInfo4 = new DirectoryInfo(Path.Combine(current.ToString(), "Strings"));
				if (directoryInfo4.Exists)
				{
					DirectoryInfo[] directories2 = directoryInfo4.GetDirectories("*", SearchOption.TopDirectoryOnly);
					for (int l = 0; l < directories2.Length; l++)
					{
						DirectoryInfo directoryInfo5 = directories2[l];
						FileInfo[] files3 = directoryInfo5.GetFiles("*.txt", SearchOption.AllDirectories);
						for (int m = 0; m < files3.Length; m++)
						{
							FileInfo file3 = files3[m];
							this.LoadFromFile_Strings(file3, directoryInfo4);
						}
					}
				}
			}
			DeepProfiler.End();
		}

		private void LoadFromFile_Strings(FileInfo file, DirectoryInfo stringsTopDir)
		{
			string text;
			try
			{
				text = GenFile.TextFromRawFile(file.FullName);
			}
			catch (Exception ex)
			{
				this.loadErrors.Add(string.Concat(new object[]
				{
					"Exception loading from strings file ",
					file,
					": ",
					ex
				}));
				return;
			}
			string text2 = file.FullName;
			if (stringsTopDir != null)
			{
				text2 = text2.Substring(stringsTopDir.FullName.Length + 1);
			}
			text2 = text2.Substring(0, text2.Length - Path.GetExtension(text2).Length);
			text2 = text2.Replace('\\', '/');
			List<string> list = new List<string>();
			foreach (string current in GenText.LinesFromString(text))
			{
				list.Add(current);
			}
			List<string> list2;
			if (this.stringFiles.TryGetValue(text2, out list2))
			{
				foreach (string current2 in list)
				{
					list2.Add(current2);
				}
			}
			else
			{
				this.stringFiles.Add(text2, list);
			}
		}

		private void LoadFromFile_Keyed(FileInfo file)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			try
			{
				foreach (DirectXmlLoaderSimple.XmlKeyValuePair current in DirectXmlLoaderSimple.ValuesFromXmlFile(file))
				{
					if (this.keyedReplacements.ContainsKey(current.key) || dictionary.ContainsKey(current.key))
					{
						this.loadErrors.Add("Duplicate keyed translation key: " + current.key + " in language " + this.folderName);
					}
					else
					{
						dictionary.Add(current.key, current.value);
						dictionary2.Add(current.key, current.lineNumber);
					}
				}
			}
			catch (Exception ex)
			{
				this.loadErrors.Add(string.Concat(new object[]
				{
					"Exception loading from translation file ",
					file,
					": ",
					ex
				}));
				dictionary.Clear();
				dictionary2.Clear();
				this.anyKeyedReplacementsXmlParseError = true;
				this.lastKeyedReplacementsXmlParseErrorInFile = file.Name;
			}
			foreach (KeyValuePair<string, string> current2 in dictionary)
			{
				string text = current2.Value;
				LoadedLanguage.KeyedReplacement keyedReplacement = new LoadedLanguage.KeyedReplacement();
				if (text == "TODO")
				{
					keyedReplacement.isPlaceholder = true;
					text = string.Empty;
				}
				keyedReplacement.key = current2.Key;
				keyedReplacement.value = text;
				keyedReplacement.fileSource = file.Name;
				keyedReplacement.fileSourceLine = dictionary2[current2.Key];
				keyedReplacement.fileSourceFullPath = file.FullName;
				this.keyedReplacements.Add(current2.Key, keyedReplacement);
			}
		}

		public void LoadFromFile_DefInject(FileInfo file, Type defType)
		{
			DefInjectionPackage defInjectionPackage = (from di in this.defInjections
			where di.defType == defType
			select di).FirstOrDefault<DefInjectionPackage>();
			if (defInjectionPackage == null)
			{
				defInjectionPackage = new DefInjectionPackage(defType);
				this.defInjections.Add(defInjectionPackage);
			}
			bool flag;
			defInjectionPackage.AddDataFromFile(file, out flag);
			if (flag)
			{
				this.anyDefInjectionsXmlParseError = true;
				this.lastDefInjectionsXmlParseErrorInFile = file.Name;
			}
		}

		private void EnsureAllDefTypesHaveDefInjectionPackage()
		{
			foreach (Type defType in GenDefDatabase.AllDefTypesWithDatabases())
			{
				if (!this.defInjections.Any((DefInjectionPackage x) => x.defType == defType))
				{
					this.defInjections.Add(new DefInjectionPackage(defType));
				}
			}
		}

		public bool HaveTextForKey(string key, bool allowPlaceholders = false)
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			LoadedLanguage.KeyedReplacement keyedReplacement;
			return key != null && this.keyedReplacements.TryGetValue(key, out keyedReplacement) && (allowPlaceholders || !keyedReplacement.isPlaceholder);
		}

		public bool TryGetTextFromKey(string key, out string translated)
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			if (key == null)
			{
				translated = key;
				return false;
			}
			LoadedLanguage.KeyedReplacement keyedReplacement;
			if (!this.keyedReplacements.TryGetValue(key, out keyedReplacement) || keyedReplacement.isPlaceholder)
			{
				translated = key;
				return false;
			}
			translated = keyedReplacement.value;
			return true;
		}

		public bool TryGetStringsFromFile(string fileName, out List<string> stringsList)
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			if (!this.stringFiles.TryGetValue(fileName, out stringsList))
			{
				stringsList = null;
				return false;
			}
			return true;
		}

		public string GetKeySourceFileAndLine(string key)
		{
			LoadedLanguage.KeyedReplacement keyedReplacement;
			if (!this.keyedReplacements.TryGetValue(key, out keyedReplacement))
			{
				return "unknown";
			}
			return keyedReplacement.fileSource + ":" + keyedReplacement.fileSourceLine;
		}

		public void InjectIntoData_BeforeImpliedDefs()
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			foreach (DefInjectionPackage current in this.defInjections)
			{
				try
				{
					current.InjectIntoDefs(false);
				}
				catch (Exception arg)
				{
					Log.Error("Critical error while injecting translations into defs: " + arg, false);
				}
			}
		}

		public void InjectIntoData_AfterImpliedDefs()
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			int num = this.loadErrors.Count;
			foreach (DefInjectionPackage current in this.defInjections)
			{
				try
				{
					current.InjectIntoDefs(true);
					num += current.loadErrors.Count;
				}
				catch (Exception arg)
				{
					Log.Error("Critical error while injecting translations into defs: " + arg, false);
				}
			}
			BackstoryTranslationUtility.LoadAndInjectBackstoryData(this.FolderPaths, this.backstoriesLoadErrors);
			num += this.backstoriesLoadErrors.Count;
			if (num != 0)
			{
				this.anyError = true;
				Log.Warning(string.Concat(new object[]
				{
					"Translation data for language ",
					LanguageDatabase.activeLanguage.FriendlyNameEnglish,
					" has ",
					num,
					" errors. Generate translation report for more info."
				}), false);
			}
		}

		public override string ToString()
		{
			return this.info.friendlyNameEnglish;
		}
	}
}
