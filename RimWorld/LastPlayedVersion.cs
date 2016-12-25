using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public static class LastPlayedVersion
	{
		private static bool initialized;

		private static Version lastPlayedVersionInt;

		private static bool gameWasRun;

		public static Version Version
		{
			get
			{
				LastPlayedVersion.InitializeIfNeeded();
				if (LastPlayedVersion.lastPlayedVersionInt == null && LastPlayedVersion.gameWasRun)
				{
					return new Version(0, 13, 1241);
				}
				return LastPlayedVersion.lastPlayedVersionInt;
			}
		}

		public static void InitializeIfNeeded()
		{
			if (LastPlayedVersion.initialized)
			{
				return;
			}
			try
			{
				string text = null;
				if (File.Exists(GenFilePaths.LastPlayedVersionFilePath))
				{
					try
					{
						text = File.ReadAllText(GenFilePaths.LastPlayedVersionFilePath);
					}
					catch (Exception ex)
					{
						Log.Error("Exception getting last played version data. Path: " + GenFilePaths.LastPlayedVersionFilePath + ". Exception: " + ex.ToString());
					}
				}
				if (text != null)
				{
					try
					{
						LastPlayedVersion.lastPlayedVersionInt = VersionControl.VersionFromString(text);
					}
					catch (Exception ex2)
					{
						Log.Error("Exception parsing last version from string '" + text + "': " + ex2.ToString());
					}
				}
				if (LastPlayedVersion.lastPlayedVersionInt != VersionControl.CurrentVersion)
				{
					File.WriteAllText(GenFilePaths.LastPlayedVersionFilePath, VersionControl.CurrentVersionString);
				}
			}
			finally
			{
				LastPlayedVersion.initialized = true;
			}
		}

		internal static void Notify_GameHasBeenRun()
		{
			LastPlayedVersion.gameWasRun = true;
		}
	}
}
