using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace Verse
{
	public static class XmlLoader
	{
		private static LoadableXmlAsset loadingAsset;

		[DebuggerHidden]
		public static IEnumerable<LoadableXmlAsset> XmlAssetsInModFolder(ModContentPack mod, string folderPath)
		{
			DirectoryInfo di = new DirectoryInfo(Path.Combine(mod.RootDir, folderPath));
			if (di.Exists)
			{
				FileInfo[] files = di.GetFiles("*.xml", SearchOption.AllDirectories);
				FileInfo[] array = files;
				for (int i = 0; i < array.Length; i++)
				{
					FileInfo file = array[i];
					LoadableXmlAsset asset = new LoadableXmlAsset(file.Name, file.Directory.FullName, File.ReadAllText(file.FullName));
					yield return asset;
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<T> LoadXmlDataInResourcesFolder<T>(string folderPath) where T : new()
		{
			XmlInheritance.Clear();
			List<LoadableXmlAsset> assets = new List<LoadableXmlAsset>();
			object[] textObjects = Resources.LoadAll<TextAsset>(folderPath);
			object[] array = textObjects;
			for (int j = 0; j < array.Length; j++)
			{
				TextAsset text = (TextAsset)array[j];
				LoadableXmlAsset asset = new LoadableXmlAsset(text.name, string.Empty, text.text);
				XmlInheritance.TryRegisterAllFrom(asset, null);
				assets.Add(asset);
			}
			XmlInheritance.Resolve();
			for (int i = 0; i < assets.Count; i++)
			{
				foreach (T item in XmlLoader.AllGameItemsFromAsset<T>(assets[i]))
				{
					yield return item;
				}
			}
			XmlInheritance.Clear();
		}

		public static T ItemFromXmlFile<T>(string filePath, bool resolveCrossRefs = true) where T : new()
		{
			if (resolveCrossRefs && CrossRefLoader.LoadingInProgress)
			{
				Log.Error("Cannot call ItemFromXmlFile with resolveCrossRefs=true while loading is already in progress.");
			}
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
			{
				return (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
			}
			T result;
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(File.ReadAllText(fileInfo.FullName));
				T t = XmlToObject.ObjectFromXml<T>(xmlDocument.DocumentElement, false);
				if (resolveCrossRefs)
				{
					CrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
				}
				result = t;
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading file at " + filePath + ". Loading defaults instead. Exception was: " + ex.ToString());
				result = ((default(T) == null) ? Activator.CreateInstance<T>() : default(T));
			}
			return result;
		}

		[DebuggerHidden]
		public static IEnumerable<Def> AllDefsFromAsset(LoadableXmlAsset asset)
		{
			if (XmlLoader.loadingAsset != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to load ",
					asset,
					" while loading ",
					XmlLoader.loadingAsset,
					". This will corrupt the internal state of DataLoader."
				}));
			}
			if (asset.xmlDoc != null)
			{
				XmlLoader.loadingAsset = asset;
				XmlNodeList assetNodes = asset.xmlDoc.DocumentElement.ChildNodes;
				bool gotData = false;
				foreach (XmlNode node in assetNodes)
				{
					if (node.NodeType == XmlNodeType.Element)
					{
						XmlAttribute abstractAtt = node.Attributes["Abstract"];
						if (abstractAtt != null && abstractAtt.Value.ToLower() == "true")
						{
							gotData = true;
						}
						else
						{
							Type defType = GenTypes.GetTypeInAnyAssembly(node.Name);
							if (defType != null)
							{
								if (typeof(Def).IsAssignableFrom(defType))
								{
									MethodInfo method = typeof(XmlToObject).GetMethod("ObjectFromXml");
									MethodInfo gen = method.MakeGenericMethod(new Type[]
									{
										defType
									});
									yield return (Def)gen.Invoke(null, new object[]
									{
										node,
										true
									});
								}
							}
						}
					}
				}
				if (!gotData)
				{
					Log.Error("Found no usable data when trying to get defs from file " + asset.name);
				}
				XmlLoader.loadingAsset = null;
			}
		}

		[DebuggerHidden]
		public static IEnumerable<T> AllGameItemsFromAsset<T>(LoadableXmlAsset asset) where T : new()
		{
			if (XmlLoader.loadingAsset != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to load ",
					asset,
					" while loading ",
					XmlLoader.loadingAsset,
					". This will corrupt the internal state of DataLoader."
				}));
			}
			if (asset.xmlDoc != null)
			{
				XmlLoader.loadingAsset = asset;
				XmlNodeList assetNodes = asset.xmlDoc.DocumentElement.SelectNodes(typeof(T).Name);
				bool gotData = false;
				foreach (XmlNode node in assetNodes)
				{
					XmlAttribute abstractAtt = node.Attributes["Abstract"];
					if (abstractAtt == null || !(abstractAtt.Value.ToLower() == "true"))
					{
						yield return XmlToObject.ObjectFromXml<T>(node, true);
					}
				}
				if (!gotData)
				{
					Log.Error(string.Concat(new object[]
					{
						"Found no usable data when trying to get ",
						typeof(T),
						"s from file ",
						asset.name
					}));
				}
				XmlLoader.loadingAsset = null;
			}
		}
	}
}
