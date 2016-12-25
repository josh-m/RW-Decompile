using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GlobalControlsUtility
	{
		private const int VisibilityControlsPerRow = 5;

		public static void DoPlaySettings(WidgetRow rowVisibility, bool worldView, ref float curBaseY)
		{
			float y = curBaseY - TimeControls.TimeButSize.y;
			rowVisibility.Init((float)UI.screenWidth, y, UIDirection.LeftThenUp, 141f, 4f);
			Find.PlaySettings.DoPlaySettingsGlobalControls(rowVisibility, worldView);
			curBaseY = rowVisibility.FinalY;
		}

		public static void DoTimespeedControls(float leftX, float width, ref float curBaseY)
		{
			float y = TimeControls.TimeButSize.y;
			Rect timerRect = new Rect(leftX + 16f, curBaseY - y, width, y);
			TimeControls.DoTimeControlsGUI(timerRect);
			curBaseY -= timerRect.height;
		}

		public static void DoDate(float leftX, float width, ref float curBaseY)
		{
			Rect dateRect = new Rect(leftX, curBaseY - 48f, width, 48f);
			DateReadout.DateOnGUI(dateRect);
			curBaseY -= dateRect.height;
		}

		public static void DoRealtimeClock(float leftX, float width, ref float curBaseY)
		{
			Rect rect = new Rect(leftX - 20f, curBaseY - 26f, width + 20f - 7f, 26f);
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, DateTime.Now.ToString("HH:mm"));
			Text.Anchor = TextAnchor.UpperLeft;
			curBaseY -= 26f;
		}
	}
}
