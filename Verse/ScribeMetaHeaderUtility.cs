using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Verse
{
	public class ScribeMetaHeaderUtility
	{
		public enum ScribeHeaderMode
		{
			None,
			Map,
			World,
			Scenario
		}

		public const string MetaNodeName = "meta";

		public const string GameVersionNodeName = "gameVersion";

		public const string ModIdsNodeName = "modIds";

		public const string ModNamesNodeName = "modNames";

		private static ScribeMetaHeaderUtility.ScribeHeaderMode lastMode;

		public static string loadedGameVersion;

		public static List<string> loadedModIdsList;

		public static List<string> loadedModNamesList;

		public static void WriteMetaHeader()
		{
			Scribe.EnterNode("meta");
			string currentVersionStringWithRev = VersionControl.CurrentVersionStringWithRev;
			Scribe_Values.LookValue<string>(ref currentVersionStringWithRev, "gameVersion", null, false);
			List<string> list = (from mod in LoadedModManager.RunningMods
			select mod.Identifier).ToList<string>();
			Scribe_Collections.LookList<string>(ref list, "modIds", LookMode.Undefined, new object[0]);
			List<string> list2 = (from mod in LoadedModManager.RunningMods
			select mod.Name).ToList<string>();
			Scribe_Collections.LookList<string>(ref list2, "modNames", LookMode.Undefined, new object[0]);
			Scribe.ExitNode();
		}

		public static void LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode mode, bool logVersionConflictWarning)
		{
			ScribeMetaHeaderUtility.loadedGameVersion = "Unknown";
			ScribeMetaHeaderUtility.loadedModIdsList = null;
			ScribeMetaHeaderUtility.loadedModNamesList = null;
			ScribeMetaHeaderUtility.lastMode = mode;
			if (Scribe.mode != LoadSaveMode.Inactive && Scribe.EnterNode("meta"))
			{
				Scribe_Values.LookValue<string>(ref ScribeMetaHeaderUtility.loadedGameVersion, "gameVersion", null, false);
				Scribe_Collections.LookList<string>(ref ScribeMetaHeaderUtility.loadedModIdsList, "modIds", LookMode.Undefined, new object[0]);
				Scribe_Collections.LookList<string>(ref ScribeMetaHeaderUtility.loadedModNamesList, "modNames", LookMode.Undefined, new object[0]);
				Scribe.ExitNode();
			}
			if (logVersionConflictWarning && (mode == ScribeMetaHeaderUtility.ScribeHeaderMode.Map || !UnityData.isEditor) && !ScribeMetaHeaderUtility.VersionsMatch())
			{
				Log.Warning(string.Concat(new string[]
				{
					"Loaded file is from version ",
					ScribeMetaHeaderUtility.loadedGameVersion,
					", we are running version ",
					VersionControl.CurrentVersionStringWithRev,
					"."
				}));
			}
		}

		private static bool VersionsMatch()
		{
			return VersionControl.BuildFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) == VersionControl.BuildFromVersionString(VersionControl.CurrentVersionStringWithRev);
		}

		public static bool TryCreateDialogsForVersionMismatchWarnings(Action confirmedAction)
		{
			string text = null;
			string text2 = null;
			if (!ScribeMetaHeaderUtility.VersionsMatch())
			{
				text2 = "VersionMismatch".Translate();
				string text3 = (!ScribeMetaHeaderUtility.loadedGameVersion.NullOrEmpty()) ? ScribeMetaHeaderUtility.loadedGameVersion : ("(" + "UnknownLower".Translate() + ")");
				if (ScribeMetaHeaderUtility.lastMode == ScribeMetaHeaderUtility.ScribeHeaderMode.Map)
				{
					text = "SaveGameIncompatibleWarningText".Translate(new object[]
					{
						text3,
						VersionControl.CurrentVersionString
					});
				}
				else if (ScribeMetaHeaderUtility.lastMode == ScribeMetaHeaderUtility.ScribeHeaderMode.World)
				{
					text = "WorldFileVersionMismatch".Translate(new object[]
					{
						text3,
						VersionControl.CurrentVersionString
					});
				}
				else
				{
					text = "FileIncompatibleWarning".Translate(new object[]
					{
						text3,
						VersionControl.CurrentVersionString
					});
				}
			}
			string text4;
			string text5;
			if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out text4, out text5))
			{
				string text6 = "ModsMismatchWarningText".Translate(new object[]
				{
					text4,
					text5
				});
				if (text == null)
				{
					text = text6;
				}
				else
				{
					text = text + "\n\n" + text6;
				}
				if (text2 == null)
				{
					text2 = "ModsMismatchWarningTitle".Translate();
				}
			}
			if (text != null)
			{
				string title = text2;
				Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation(text, confirmedAction, false, title);
				dialog_MessageBox.buttonAText = "LoadAnyway".Translate();
				Find.WindowStack.Add(dialog_MessageBox);
				return true;
			}
			return false;
		}

		public static bool LoadedModsMatchesActiveMods(out string loadedModsSummary, out string runningModsSummary)
		{
			loadedModsSummary = null;
			runningModsSummary = null;
			List<string> list = (from mod in LoadedModManager.RunningMods
			select mod.Identifier).ToList<string>();
			if (ScribeMetaHeaderUtility.ModListsMatch(ScribeMetaHeaderUtility.loadedModIdsList, list))
			{
				return true;
			}
			if (ScribeMetaHeaderUtility.loadedModNamesList == null)
			{
				loadedModsSummary = "None".Translate();
			}
			else
			{
				loadedModsSummary = GenText.ToCommaList(ScribeMetaHeaderUtility.loadedModNamesList, true);
			}
			runningModsSummary = GenText.ToCommaList(from id in list
			select ModLister.GetModWithIdentifier(id).Name, true);
			return false;
		}

		private static bool ModListsMatch(List<string> a, List<string> b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			if (a.Count != b.Count)
			{
				return false;
			}
			for (int i = 0; i < a.Count; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string GameVersionOf(FileInfo file)
		{
			if (!file.Exists)
			{
				throw new ArgumentException();
			}
			try
			{
				using (StreamReader streamReader = new StreamReader(file.FullName))
				{
					using (XmlTextReader xmlTextReader = new XmlTextReader(streamReader))
					{
						if (ScribeMetaHeaderUtility.ReadToMetaElement(xmlTextReader) && xmlTextReader.ReadToDescendant("gameVersion"))
						{
							return VersionControl.VersionStringWithoutRev(xmlTextReader.ReadString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception getting game version of " + file.Name + ": " + ex.ToString());
			}
			return null;
		}

		public static bool ReadToMetaElement(XmlTextReader textReader)
		{
			return ScribeMetaHeaderUtility.ReadToNextElement(textReader) && ScribeMetaHeaderUtility.ReadToNextElement(textReader) && !(textReader.Name != "meta");
		}

		private static bool ReadToNextElement(XmlTextReader textReader)
		{
			while (textReader.Read())
			{
				if (textReader.NodeType == XmlNodeType.Element)
				{
					return true;
				}
			}
			return false;
		}
	}
}
