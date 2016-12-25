using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ResolutionUtility
	{
		public const int MinResolutionWidth = 1024;

		public const int MinResolutionHeight = 768;

		public static void SafeSetResolution(Resolution res)
		{
			if (Screen.width == res.width && Screen.height == res.height)
			{
				return;
			}
			IntVec2 oldRes = new IntVec2(Screen.width, Screen.height);
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
			Screen.fullScreen = fullScreen;
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(fullScreen2));
		}

		public static void SafeSetUIScale(float newScale)
		{
			if (Prefs.UIScale == newScale)
			{
				return;
			}
			float uIScale = Prefs.UIScale;
			Prefs.UIScale = newScale;
			Find.WindowStack.Add(new Dialog_ResolutionConfirm(uIScale));
		}

		public static bool UIScaleSafeWithResolution(float scale, Resolution res)
		{
			return (float)res.width / scale >= 1024f && (float)res.height / scale >= 768f;
		}
	}
}
