using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ResolutionUtility
	{
		public const int MinResolutionWidth = 1024;

		public const int MinResolutionHeight = 768;

		public static Resolution NativeResolution
		{
			get
			{
				return (from x in Screen.resolutions
				orderby x.width descending, x.height descending
				select x).First<Resolution>();
			}
		}

		public static void SafeSetResolution(Resolution res)
		{
			if (Screen.width == res.width && Screen.height == res.height)
			{
				return;
			}
			IntVec2 oldRes = new IntVec2(Screen.width, Screen.height);
			ResolutionUtility.SetResolutionRaw(res.width, res.height, Screen.fullScreen);
			Prefs.ScreenWidth = res.width;
			Prefs.ScreenHeight = res.height;
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
			Prefs.FullScreen = fullScreen;
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

		public static bool UIScaleSafeWithResolution(float scale, int w, int h)
		{
			return (float)w / scale >= 1024f && (float)h / scale >= 768f;
		}

		public static void SetResolutionRaw(int w, int h, bool fullScreen)
		{
			Resolution currentResolution = Screen.currentResolution;
			if (currentResolution.width != w || currentResolution.height != h || Screen.fullScreen != fullScreen)
			{
				Screen.SetResolution(w, h, fullScreen);
			}
		}

		public static void SetNativeResolutionRaw()
		{
			Resolution nativeResolution = ResolutionUtility.NativeResolution;
			ResolutionUtility.SetResolutionRaw(nativeResolution.width, nativeResolution.height, true);
		}

		public static void Update()
		{
			if (RealTime.frameCount % 30 == 0 && !Screen.fullScreen)
			{
				bool flag = false;
				if (Screen.width != Prefs.ScreenWidth)
				{
					Prefs.ScreenWidth = Screen.width;
					flag = true;
				}
				if (Screen.height != Prefs.ScreenHeight)
				{
					Prefs.ScreenHeight = Screen.height;
					flag = true;
				}
				if (flag)
				{
					Prefs.Save();
				}
			}
		}
	}
}
