using RimWorld;
using Steamworks;
using System;

namespace Verse
{
	public static class ScreenshotTaker
	{
		private static bool takeScreenshot;

		public static void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			if (KeyBindingDefOf.TakeScreenshot.JustPressed || ScreenshotTaker.takeScreenshot)
			{
				ScreenshotTaker.TakeShot();
				ScreenshotTaker.takeScreenshot = false;
			}
		}

		public static void QueueSilentScreenshot()
		{
			ScreenshotTaker.takeScreenshot = true;
		}

		private static void TakeShot()
		{
			SteamScreenshots.TriggerScreenshot();
		}
	}
}
