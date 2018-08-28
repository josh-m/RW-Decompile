using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

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
			LoadedModManager.InitializeMods();
			LoadedModManager.LoadModContent();
			LoadedModManager.CreateModClasses();
			List<LoadableXmlAsset> xmls = LoadedModManager.LoadModXML();
			Dictionary<XmlNode, LoadableXmlAsset> assetlookup = new Dictionary<XmlNode, LoadableXmlAsset>();
			XmlDocument xmlDoc = LoadedModManager.CombineIntoUnifiedXML(xmls, assetlookup);
			LoadedModManager.ApplyPatches(xmlDoc, assetlookup);
			LoadedModManager.ParseAndProcessXML(xmlDoc, assetlookup);
			LoadedModManager.ClearCachedPatches();
			XmlInheritance.Clear();
		}

		public static void InitializeMods()
		{
			int num = 0;
			foreach (ModMetaData current in ModsConfig.ActiveModsInLoadOrder.ToList<ModMetaData>())
			{
				DeepProfiler.Start("Initializing " + current);
				try
				{
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
						}), false);
					}
					else
					{
						ModContentPack item = new ModContentPack(current.RootDir, num, current.Name);
						num++;
						LoadedModManager.runningMods.Add(item);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Error initializing mod: " + arg, false);
					ModsConfig.SetActive(current.Identifier, false);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static void LoadModContent()
		{
			for (int i = 0; i < LoadedModManager.runningMods.Count; i++)
			{
				ModContentPack modContentPack = LoadedModManager.runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack + " content");
				try
				{
					modContentPack.ReloadContent();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not reload mod content for mod ",
						modContentPack.Identifier,
						": ",
						ex
					}), false);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static void CreateModClasses()
		{
			foreach (Type type in typeof(Mod).InstantiableDescendantsAndSelf())
			{
				try
				{
					if (!LoadedModManager.runningModClasses.ContainsKey(type))
					{
						ModContentPack modContentPack = (from modpack in LoadedModManager.runningMods
						where modpack.assemblies.loadedAssemblies.Contains(type.Assembly)
						select modpack).FirstOrDefault<ModContentPack>();
						LoadedModManager.runningModClasses[type] = (Mod)Activator.CreateInstance(type, new object[]
						{
							modContentPack
						});
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Error while instantiating a mod of type ",
						type,
						": ",
						ex
					}), false);
				}
			}
		}

		public static List<LoadableXmlAsset> LoadModXML()
		{
			List<LoadableXmlAsset> list = new List<LoadableXmlAsset>();
			for (int i = 0; i < LoadedModManager.runningMods.Count; i++)
			{
				ModContentPack modContentPack = LoadedModManager.runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack);
				try
				{
					list.AddRange(modContentPack.LoadDefs());
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not load defs for mod ",
						modContentPack.Identifier,
						": ",
						ex
					}), false);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			return list;
		}

		public static void ApplyPatches(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			foreach (PatchOperation current in LoadedModManager.runningMods.SelectMany((ModContentPack rm) => rm.Patches))
			{
				try
				{
					current.Apply(xmlDoc);
				}
				catch (Exception arg)
				{
					Log.Error("Error in patch.Apply(): " + arg, false);
				}
			}
		}

		public static XmlDocument CombineIntoUnifiedXML(List<LoadableXmlAsset> xmls, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateElement("Defs"));
			foreach (LoadableXmlAsset current in xmls)
			{
				if (current.xmlDoc == null || current.xmlDoc.DocumentElement == null)
				{
					Log.Error(string.Format("{0}: unknown parse failure", current.fullFolderPath + "/" + current.name), false);
				}
				else
				{
					if (current.xmlDoc.DocumentElement.Name != "Defs")
					{
						Log.Error(string.Format("{0}: root element named {1}; should be named Defs", current.fullFolderPath + "/" + current.name, current.xmlDoc.DocumentElement.Name), false);
					}
					foreach (XmlNode node in current.xmlDoc.DocumentElement.ChildNodes)
					{
						XmlNode xmlNode = xmlDocument.ImportNode(node, true);
						assetlookup[xmlNode] = current;
						xmlDocument.DocumentElement.AppendChild(xmlNode);
					}
				}
			}
			return xmlDocument;
		}

		public static void ParseAndProcessXML(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
			for (int i = 0; i < childNodes.Count; i++)
			{
				if (childNodes[i].NodeType == XmlNodeType.Element)
				{
					LoadableXmlAsset loadableXmlAsset = assetlookup.TryGetValue(childNodes[i], null);
					XmlInheritance.TryRegister(childNodes[i], (loadableXmlAsset == null) ? null : loadableXmlAsset.mod);
				}
			}
			XmlInheritance.Resolve();
			DefPackage defPackage = new DefPackage("Unknown", string.Empty);
			ModContentPack modContentPack = LoadedModManager.runningMods.FirstOrDefault<ModContentPack>();
			modContentPack.AddDefPackage(defPackage);
			foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
			{
				LoadableXmlAsset loadableXmlAsset2 = assetlookup.TryGetValue(xmlNode, null);
				DefPackage defPackage2 = (loadableXmlAsset2 == null) ? defPackage : loadableXmlAsset2.defPackage;
				Def def = DirectXmlLoader.DefFromNode(xmlNode, loadableXmlAsset2);
				if (def != null)
				{
					def.modContentPack = ((loadableXmlAsset2 == null) ? modContentPack : loadableXmlAsset2.mod);
					defPackage2.AddDef(def);
				}
			}
		}

		public static void ClearCachedPatches()
		{
			foreach (ModContentPack current in LoadedModManager.runningMods)
			{
				foreach (PatchOperation current2 in current.Patches)
				{
					try
					{
						current2.Complete(current.Name);
					}
					catch (Exception arg)
					{
						Log.Error("Error in patch.Complete(): " + arg, false);
					}
				}
				current.ClearPatchesCache();
			}
		}

		public static void ClearDestroy()
		{
			foreach (ModContentPack current in LoadedModManager.runningMods)
			{
				try
				{
					current.ClearDestroy();
				}
				catch (Exception arg)
				{
					Log.Error("Error in mod.ClearDestroy(): " + arg, false);
				}
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
					try
					{
						Scribe_Deep.Look<T>(ref t, "ModSettings", new object[0]);
					}
					finally
					{
						Scribe.loader.FinalizeLoading();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("Caught exception while loading mod settings data for {0}. Generating fresh settings. The exception was: {1}", modIdentifier, ex.ToString()), false);
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
			try
			{
				Scribe_Deep.Look<ModSettings>(ref settings, "ModSettings", new object[0]);
			}
			finally
			{
				Scribe.saver.FinalizeSaving();
			}
		}
	}
}
