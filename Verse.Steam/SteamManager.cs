using Steamworks;
using System;
using System.Text;
using UnityEngine;

namespace Verse.Steam
{
	public static class SteamManager
	{
		private static SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;

		private static bool initializedInt;

		public static bool Initialized
		{
			get
			{
				return SteamManager.initializedInt;
			}
		}

		public static bool Active
		{
			get
			{
				return true;
			}
		}

		public static void InitIfNeeded()
		{
			if (SteamManager.initializedInt)
			{
				return;
			}
			if (!Packsize.Test())
			{
				Log.Error("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}
			if (!DllCheck.Test())
			{
				Log.Error("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}
			try
			{
				if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
				{
					Application.Quit();
					return;
				}
			}
			catch (DllNotFoundException arg)
			{
				Log.Error("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + arg);
				Application.Quit();
				return;
			}
			SteamManager.initializedInt = SteamAPI.Init();
			if (!SteamManager.initializedInt)
			{
				Log.Warning("[Steamworks.NET] SteamAPI.Init() failed. Possible causes: Steam client not running, launched from outside Steam without steam_appid.txt in place, running with different privileges than Steam client (e.g. \"as administrator\")");
			}
			else
			{
				if (SteamManager.steamAPIWarningMessageHook == null)
				{
					SteamManager.steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIDebugTextHook);
					SteamClient.SetWarningMessageHook(SteamManager.steamAPIWarningMessageHook);
				}
				Workshop.Init();
			}
		}

		public static void Update()
		{
			if (!SteamManager.initializedInt)
			{
				return;
			}
			SteamAPI.RunCallbacks();
		}

		public static void ShutdownSteam()
		{
			if (!SteamManager.initializedInt)
			{
				return;
			}
			SteamAPI.Shutdown();
		}

		private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Log.Error(pchDebugText.ToString());
		}
	}
}
