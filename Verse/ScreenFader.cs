using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ScreenFader
	{
		private static GUIStyle backgroundStyle;

		private static Texture2D fadeTexture;

		private static Color sourceColor;

		private static Color targetColor;

		private static float sourceTime;

		private static float targetTime;

		private static bool fadeTextureDirty;

		private static float CurTime
		{
			get
			{
				return Time.realtimeSinceStartup;
			}
		}

		static ScreenFader()
		{
			ScreenFader.backgroundStyle = new GUIStyle();
			ScreenFader.sourceColor = new Color(0f, 0f, 0f, 0f);
			ScreenFader.targetColor = new Color(0f, 0f, 0f, 0f);
			ScreenFader.sourceTime = 0f;
			ScreenFader.targetTime = 0f;
			ScreenFader.fadeTextureDirty = true;
			ScreenFader.fadeTexture = new Texture2D(1, 1);
			ScreenFader.fadeTexture.name = "ScreenFader";
			ScreenFader.backgroundStyle.normal.background = ScreenFader.fadeTexture;
			ScreenFader.fadeTextureDirty = true;
		}

		public static void OverlayOnGUI(Vector2 windowSize)
		{
			Color color = ScreenFader.CurrentInstantColor();
			if (color.a > 0f)
			{
				if (ScreenFader.fadeTextureDirty)
				{
					ScreenFader.fadeTexture.SetPixel(0, 0, color);
					ScreenFader.fadeTexture.Apply();
				}
				GUI.Label(new Rect(-10f, -10f, windowSize.x + 10f, windowSize.y + 10f), ScreenFader.fadeTexture, ScreenFader.backgroundStyle);
			}
		}

		private static Color CurrentInstantColor()
		{
			if (ScreenFader.CurTime > ScreenFader.targetTime || ScreenFader.targetTime == ScreenFader.sourceTime)
			{
				return ScreenFader.targetColor;
			}
			return Color.Lerp(ScreenFader.sourceColor, ScreenFader.targetColor, (ScreenFader.CurTime - ScreenFader.sourceTime) / (ScreenFader.targetTime - ScreenFader.sourceTime));
		}

		public static void SetColor(Color newColor)
		{
			ScreenFader.sourceColor = newColor;
			ScreenFader.targetColor = newColor;
			ScreenFader.targetTime = 0f;
			ScreenFader.sourceTime = 0f;
			ScreenFader.fadeTextureDirty = true;
		}

		public static void StartFade(Color finalColor, float duration)
		{
			if (duration <= 0f)
			{
				ScreenFader.SetColor(finalColor);
				return;
			}
			ScreenFader.sourceColor = ScreenFader.CurrentInstantColor();
			ScreenFader.targetColor = finalColor;
			ScreenFader.sourceTime = ScreenFader.CurTime;
			ScreenFader.targetTime = ScreenFader.CurTime + duration;
		}
	}
}
