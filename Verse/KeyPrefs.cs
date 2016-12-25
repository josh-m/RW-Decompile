using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Verse
{
	public class KeyPrefs
	{
		public enum BindingSlot : byte
		{
			A,
			B
		}

		private static KeyPrefsData data;

		private static Dictionary<string, KeyBindingData> unresolvedBindings;

		public static KeyPrefsData KeyPrefsData
		{
			get
			{
				return KeyPrefs.data;
			}
			set
			{
				KeyPrefs.data = value;
			}
		}

		public static void Init()
		{
			bool flag = !new FileInfo(GenFilePaths.KeyPrefsFilePath).Exists;
			Dictionary<string, KeyBindingData> dictionary = XmlLoader.ItemFromXmlFile<Dictionary<string, KeyBindingData>>(GenFilePaths.KeyPrefsFilePath, true);
			KeyPrefs.data = new KeyPrefsData();
			KeyPrefs.unresolvedBindings = new Dictionary<string, KeyBindingData>();
			foreach (KeyValuePair<string, KeyBindingData> current in dictionary)
			{
				KeyBindingDef namedSilentFail = DefDatabase<KeyBindingDef>.GetNamedSilentFail(current.Key);
				if (namedSilentFail != null)
				{
					KeyPrefs.data.keyPrefs[namedSilentFail] = current.Value;
				}
				else
				{
					KeyPrefs.unresolvedBindings[current.Key] = current.Value;
				}
			}
			if (flag)
			{
				KeyPrefs.data.ResetToDefaults();
			}
			KeyPrefs.data.AddMissingDefaultBindings();
			KeyPrefs.data.ErrorCheck();
			if (flag)
			{
				KeyPrefs.Save();
			}
		}

		public static void Save()
		{
			try
			{
				Dictionary<string, KeyBindingData> dictionary = new Dictionary<string, KeyBindingData>();
				foreach (KeyValuePair<KeyBindingDef, KeyBindingData> current in KeyPrefs.data.keyPrefs)
				{
					dictionary[current.Key.defName] = current.Value;
				}
				foreach (KeyValuePair<string, KeyBindingData> current2 in KeyPrefs.unresolvedBindings)
				{
					try
					{
						dictionary.Add(current2.Key, current2.Value);
					}
					catch (ArgumentException)
					{
					}
				}
				XDocument xDocument = new XDocument();
				XElement content = XmlSaver.XElementFromObject(dictionary, typeof(KeyPrefsData));
				xDocument.Add(content);
				xDocument.Save(GenFilePaths.KeyPrefsFilePath);
			}
			catch (Exception ex)
			{
				GenUI.ErrorDialog("ProblemSavingFile".Translate(new object[]
				{
					GenFilePaths.KeyPrefsFilePath,
					ex.ToString()
				}));
				Log.Error("Exception saving keyprefs: " + ex);
			}
		}
	}
}
