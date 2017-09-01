using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class LoadedLanguage
	{
		public string folderName;

		public LanguageInfo info;

		private LanguageWorker workerInt;

		private bool dataIsLoaded;

		public Texture2D icon = BaseContent.BadTex;

		public Dictionary<string, string> keyedReplacements = new Dictionary<string, string>();

		public List<DefInjectionPackage> defInjections = new List<DefInjectionPackage>();

		public Dictionary<string, List<string>> stringFiles = new Dictionary<string, List<string>>();

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
					Log.Warning("Translations aren't called CodeLinked any more. Please rename to Keyed: " + directoryInfo);
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
					Log.Warning("Translations aren't called DefLinked any more. Please rename to DefInjected: " + directoryInfo2);
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
							Log.Warning(string.Concat(new string[]
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
				Log.Warning(string.Concat(new object[]
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
			try
			{
				foreach (KeyValuePair<string, string> current in DirectXmlLoaderSimple.ValuesFromXmlFile(file))
				{
					if (this.keyedReplacements.ContainsKey(current.Key) || dictionary.ContainsKey(current.Key))
					{
						Log.Warning("Duplicate code-linked translation key: " + current.Key + " in language " + this.folderName);
					}
					else
					{
						dictionary.Add(current.Key, current.Value);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Exception loading from translation file ",
					file,
					": ",
					ex
				}));
				dictionary.Clear();
			}
			foreach (KeyValuePair<string, string> current2 in dictionary)
			{
				this.keyedReplacements.Add(current2.Key, current2.Value);
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
			defInjectionPackage.AddDataFromFile(file);
		}

		public bool HaveTextForKey(string key)
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			return this.keyedReplacements.ContainsKey(key);
		}

		public bool TryGetTextFromKey(string key, out string translated)
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			if (!this.keyedReplacements.TryGetValue(key, out translated))
			{
				translated = key;
				return false;
			}
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

		public void InjectIntoData()
		{
			if (!this.dataIsLoaded)
			{
				this.LoadData();
			}
			foreach (DefInjectionPackage current in this.defInjections)
			{
				current.InjectIntoDefs();
			}
			BackstoryTranslationUtility.LoadAndInjectBackstoryData(this);
		}

		public override string ToString()
		{
			return this.info.friendlyNameEnglish;
		}
	}
}
