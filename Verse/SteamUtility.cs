using Steamworks;
using System;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public static class SteamUtility
	{
		private static string cachedPersonaName;

		public static string SteamPersonaName
		{
			get
			{
				if (SteamManager.Initialized && SteamUtility.cachedPersonaName == null)
				{
					SteamUtility.cachedPersonaName = SteamFriends.GetPersonaName();
				}
				return (SteamUtility.cachedPersonaName == null) ? "???" : SteamUtility.cachedPersonaName;
			}
		}

		public static void OpenUrl(string url)
		{
			if (SteamUtils.IsOverlayEnabled())
			{
				SteamFriends.ActivateGameOverlayToWebPage(url);
			}
			else
			{
				Application.OpenURL(url);
			}
		}

		public static void OpenWorkshopPage(PublishedFileId_t pfid)
		{
			SteamUtility.OpenUrl(SteamUtility.SteamWorkshopPageUrl(pfid));
		}

		public static void OpenSteamWorkshopPage()
		{
			SteamUtility.OpenUrl("http://steamcommunity.com/workshop/browse/?appid=" + SteamUtils.GetAppID());
		}

		public static string SteamWorkshopPageUrl(PublishedFileId_t pfid)
		{
			return "steam://url/CommunityFilePage/" + pfid;
		}
	}
}
