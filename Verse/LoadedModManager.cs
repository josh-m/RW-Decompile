using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Verse
{
	public static class LoadedModManager
	{
		private static List<ModContentPack> runningMods = new List<ModContentPack>();

		private static Dictionary<Type, Mod> runningModClasses = new Dictionary<Type, Mod>();

		public static List<ModContentPack> RunningModsListForReading
		{
			get
			{
				return LoadedModManager.runningMods;
			}
		}

		public static IEnumerable<ModContentPack> RunningMods
		{
			get
			{
				return LoadedModManager.runningMods;
			}
		}

		public static IEnumerable<Mod> ModHandles
		{
			get
			{
				return LoadedModManager.runningModClasses.Values;
			}
		}

		public static void LoadAllActiveMods()
		{
			XmlInheritance.Clear();
			int num = 0;
			foreach (ModMetaData current in ModsConfig.ActiveModsInLoadOrder.ToList<ModMetaData>())
			{
				DeepProfiler.Start("Initializing " + current);
				if (!current.RootDir.Exists)
				{
					ModsConfig.SetActive(current.Identifier, false);
					Log.Warning(string.Concat(new object[]
					{
						"Failed to find active mod ",
						current.Name,
						"(",
						current.Identifier,
						") at ",
						current.RootDir
					}));
					DeepProfiler.End();
				}
				else
				{
					ModContentPack item = new ModContentPack(current.RootDir, num, current.Name);
					num++;
					LoadedModManager.runningMods.Add(item);
					DeepProfiler.End();
				}
			}
			for (int i = 0; i < LoadedModManager.runningMods.Count; i++)
			{
				ModContentPack modContentPack = LoadedModManager.runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack + " content");
				modContentPack.ReloadContent();
				DeepProfiler.End();
			}
			foreach (Type type in typeof(Mod).InstantiableDescendantsAndSelf())
			{
				if (!LoadedModManager.runningModClasses.ContainsKey(type))
				{
					ModContentPack modContentPack2 = (from modpack in LoadedModManager.runningMods
					where modpack.assemblies.loadedAssemblies.Contains(type.Assembly)
					select modpack).FirstOrDefault<ModContentPack>();
					LoadedModManager.runningModClasses[type] = (Mod)Activator.CreateInstance(type, new object[]
					{
						modContentPack2
					});
				}
			}
			for (int j = 0; j < LoadedModManager.runningMods.Count; j++)
			{
				ModContentPack modContentPack3 = LoadedModManager.runningMods[j];
				DeepProfiler.Start("Loading " + modContentPack3);
				modContentPack3.LoadDefs(LoadedModManager.runningMods.SelectMany((ModContentPack rm) => rm.Patches));
				DeepProfiler.End();
			}
			foreach (ModContentPack current2 in LoadedModManager.runningMods)
			{
				foreach (PatchOperation current3 in current2.Patches)
				{
					current3.Complete(current2.Name);
				}
				current2.ClearPatchesCache();
			}
			XmlInheritance.Clear();
		}

		public static void ClearDestroy()
		{
			foreach (ModContentPack current in LoadedModManager.runningMods)
			{
				current.ClearDestroy();
			}
			LoadedModManager.runningMods.Clear();
		}

		public static T GetMod<T>() where T : Mod
		{
			return LoadedModManager.GetMod(typeof(T)) as T;
		}

		public static Mod GetMod(Type type)
		{
			if (LoadedModManager.runningModClasses.ContainsKey(type))
			{
				return LoadedModManager.runningModClasses[type];
			}
			return (from kvp in LoadedModManager.runningModClasses
			where type.IsAssignableFrom(kvp.Key)
			select kvp).FirstOrDefault<KeyValuePair<Type, Mod>>().Value;
		}

		private static string GetSettingsFilename(string modIdentifier, string modHandleName)
		{
			return Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename(string.Format("Mod_{0}_{1}.xml", modIdentifier, modHandleName)));
		}

		public static T ReadModSettings<T>(string modIdentifier, string modHandleName) where T : ModSettings, new()
		{
			string settingsFilename = LoadedModManager.GetSettingsFilename(modIdentifier, modHandleName);
			T t = (T)((object)null);
			try
			{
				if (File.Exists(settingsFilename))
				{
					Scribe.loader.InitLoading(settingsFilename);
					Scribe_Deep.Look<T>(ref t, "ModSettings", new object[0]);
					Scribe.loader.FinalizeLoading();
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("Caught exception while loading mod settings data for {0}. Generating fresh settings. The exception was: {1}", modIdentifier, ex.ToString()));
				t = (T)((object)null);
			}
			if (t == null)
			{
				t = Activator.CreateInstance<T>();
			}
			return t;
		}

		public static void WriteModSettings(string modIdentifier, string modHandleName, ModSettings settings)
		{
			Scribe.saver.InitSaving(LoadedModManager.GetSettingsFilename(modIdentifier, modHandleName), "SettingsBlock");
			Scribe_Deep.Look<ModSettings>(ref settings, "ModSettings", new object[0]);
			Scribe.saver.FinalizeSaving();
		}
	}
}
