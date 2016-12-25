using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ResolutionUtility
	{
		public static void SafeSetResolution(Resolution res)
		{
			if (Screen.width == res.width && Screen.height == res.height)
			{
				return;
			}
			IntVec2 oldRes = new IntVec2(Screen.width, Screen.height);
			if (Prefs.LogVerbose)
			{
				Log.Message(string.Concat(new object[]
				{
					"Setting resolution to ",
					res.width,
					"x",
					res.height
				}));
			}
			Screen.SetResolution(res.width, res.height, Screen.fullScreen);
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(oldRes));
		}

		public static void SafeSetFullscreen(bool fullScreen)
		{
			if (Screen.fullScreen == fullScreen)
			{
				return;
			}
			bool fullScreen2 = Screen.fullScreen;
			if (Prefs.LogVerbose)
			{
				Log.Message("Setting fullscreen to " + fullScreen);
			}
			Screen.fullScreen = fullScreen;
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(fullScreen2));
		}
	}
}
