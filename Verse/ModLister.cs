using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse.Steam;

namespace Verse
{
	public static class ModLister
	{
		private static List<ModMetaData> mods;

		public static IEnumerable<ModMetaData> AllInstalledMods
		{
			get
			{
				return ModLister.mods;
			}
		}

		public static IEnumerable<DirectoryInfo> AllActiveModDirs
		{
			get
			{
				return from mod in ModLister.mods
				where mod.Active
				select mod.RootDir;
			}
		}

		static ModLister()
		{
			ModLister.mods = new List<ModMetaData>();
			ModLister.RebuildModList();
		}

		internal static void EnsureInit()
		{
		}

		internal static void RebuildModList()
		{
			string s = "Rebuilding mods list";
			int num = ModLister.InstalledModsListHash(false);
			ModLister.mods.Clear();
			s += "\nAdding mods from mods folder:";
			foreach (string current in from d in new DirectoryInfo(GenFilePaths.CoreModsFolderPath).GetDirectories()
			select d.FullName)
			{
				ModMetaData modMetaData = new ModMetaData(current);
				ModLister.mods.Add(modMetaData);
				s = s + "\n  Adding " + modMetaData.ToStringLong();
			}
			s += "\nAdding mods from Steam:";
			foreach (WorkshopItem current2 in from it in WorkshopItems.AllSubscribedItems
			where it is WorkshopItem_Mod
			select it)
			{
				ModMetaData modMetaData2 = new ModMetaData(current2);
				ModLister.mods.Add(modMetaData2);
				s = s + "\n  Adding " + modMetaData2.ToStringLong();
			}
			s += "\nDeactivating not-installed mods:";
			ModsConfig.DeactivateNotInstalledMods(delegate(string log)
			{
				s = s + "\n   " + log;
			});
			if (ModLister.mods.Count((ModMetaData m) => m.Active) == 0)
			{
				s += "\nThere are no active mods. Activating Core mod.";
				ModLister.mods.First((ModMetaData m) => m.IsCoreMod).Active = true;
			}
			if (Prefs.LogVerbose)
			{
				Log.Message(s);
			}
			if (ModLister.InstalledModsListHash(false) != num && !LongEventHandler.ShouldWaitForEvent)
			{
				Page_ModsConfig page_ModsConfig = Find.WindowStack.WindowOfType<Page_ModsConfig>();
				if (page_ModsConfig != null)
				{
					page_ModsConfig.Notify_ModsListChanged();
				}
			}
		}

		public static int InstalledModsListHash(bool activeOnly)
		{
			int num = 17;
			for (int i = 0; i < ModLister.mods.Count; i++)
			{
				if (!activeOnly || ModLister.mods[i].Active)
				{
					num = num * 31 + ModLister.mods[i].GetHashCode();
					num = num * 31 + i * 2654241;
				}
			}
			return num;
		}

		internal static ModMetaData GetModWithIdentifier(string identifier)
		{
			for (int i = 0; i < ModLister.mods.Count; i++)
			{
				if (ModLister.mods[i].Identifier == identifier)
				{
					return ModLister.mods[i];
				}
			}
			return null;
		}
	}
}
