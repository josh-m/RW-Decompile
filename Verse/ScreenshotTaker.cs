using RimWorld;
using Steamworks;
using System;

namespace Verse
{
	public static class ScreenshotTaker
	{
		public static void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			if (KeyBindingDefOf.TakeScreenshot.JustPressed)
			{
				ScreenshotTaker.TakeShot();
			}
		}

		private static void TakeShot()
		{
			SteamScreenshots.TriggerScreenshot();
		}
	}
}
