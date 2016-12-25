using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Verse.Steam
{
	public static class WorkshopItems
	{
		private static List<WorkshopItem> subbedItems;

		public static IEnumerable<WorkshopItem> AllSubscribedItems
		{
			get
			{
				return WorkshopItems.subbedItems;
			}
		}

		public static int DownloadingItemsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < WorkshopItems.subbedItems.Count; i++)
				{
					if (WorkshopItems.subbedItems[i] is WorkshopItem_NotInstalled)
					{
						num++;
					}
				}
				return num;
			}
		}

		static WorkshopItems()
		{
			WorkshopItems.subbedItems = new List<WorkshopItem>();
			WorkshopItems.RebuildItemsList();
		}

		public static WorkshopItem GetItem(PublishedFileId_t pfid)
		{
			for (int i = 0; i < WorkshopItems.subbedItems.Count; i++)
			{
				if (WorkshopItems.subbedItems[i].PublishedFileId == pfid)
				{
					return WorkshopItems.subbedItems[i];
				}
			}
			return null;
		}

		public static bool HasItem(PublishedFileId_t pfid)
		{
			return WorkshopItems.GetItem(pfid) != null;
		}

		private static void RebuildItemsList()
		{
			if (!SteamManager.Initialized)
			{
				return;
			}
			WorkshopItems.subbedItems.Clear();
			foreach (PublishedFileId_t current in Workshop.AllSubscribedItems())
			{
				WorkshopItems.subbedItems.Add(WorkshopItem.MakeFrom(current));
			}
			ModLister.RebuildModList();
			ScenarioLister.MarkDirty();
		}

		internal static void Notify_Subscribed(PublishedFileId_t pfid)
		{
			WorkshopItems.RebuildItemsList();
		}

		internal static void Notify_Installed(PublishedFileId_t pfid)
		{
			WorkshopItems.RebuildItemsList();
		}

		internal static void Notify_Unsubscribed(PublishedFileId_t pfid)
		{
			WorkshopItems.RebuildItemsList();
		}

		public static string DebugOutput()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Subscribed items:");
			foreach (WorkshopItem current in WorkshopItems.subbedItems)
			{
				stringBuilder.AppendLine("  " + current.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
