using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DateReadout
	{
		public const float Height = 48f;

		private static string dateString;

		private static int dateStringDay = -1;

		private static readonly List<string> fastHourStrings = new List<string>();

		public static void Reinit()
		{
			DateReadout.dateString = null;
			DateReadout.dateStringDay = -1;
			DateReadout.fastHourStrings.Clear();
			for (int i = 0; i < 24; i++)
			{
				DateReadout.fastHourStrings.Add(i + "LetterHour".Translate());
			}
		}

		public static void DateOnGUI(Rect dateRect)
		{
			if (Mouse.IsOver(dateRect))
			{
				Widgets.DrawHighlight(dateRect);
			}
			GUI.BeginGroup(dateRect);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect = dateRect.AtZero();
			rect.xMax -= 7f;
			Widgets.Label(rect, DateReadout.fastHourStrings[GenDate.HourInt]);
			rect.yMin += 26f;
			if (GenDate.DayOfMonth != DateReadout.dateStringDay)
			{
				DateReadout.dateString = GenDate.DateReadoutStringAt(Find.TickManager.TicksAbs);
				DateReadout.dateStringDay = GenDate.DayOfMonth;
			}
			Widgets.Label(rect, DateReadout.dateString);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			TooltipHandler.TipRegion(dateRect, new TipSignal(() => "DateReadoutTip".Translate(new object[]
			{
				GenDate.DaysPassed,
				15,
				GenDate.CurrentSeason.Label()
			}), 86423));
		}
	}
}
