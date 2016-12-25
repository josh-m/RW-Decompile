using RimWorld.Planet;
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

		private static int dateStringDay;

		private static Season dateStringSeason;

		private static int dateStringYear;

		private static readonly List<string> fastHourStrings;

		static DateReadout()
		{
			DateReadout.dateStringDay = -1;
			DateReadout.dateStringSeason = Season.Undefined;
			DateReadout.dateStringYear = -1;
			DateReadout.fastHourStrings = new List<string>();
			DateReadout.Reset();
		}

		public static void Reset()
		{
			DateReadout.dateString = null;
			DateReadout.dateStringDay = -1;
			DateReadout.dateStringSeason = Season.Undefined;
			DateReadout.dateStringYear = -1;
			DateReadout.fastHourStrings.Clear();
			for (int i = 0; i < 24; i++)
			{
				DateReadout.fastHourStrings.Add(i + "LetterHour".Translate());
			}
		}

		public static void DateOnGUI(Rect dateRect)
		{
			float x;
			if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.selectedTile >= 0)
			{
				x = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile).x;
			}
			else if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.NumSelectedObjects > 0)
			{
				x = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile).x;
			}
			else
			{
				if (Find.VisibleMap == null)
				{
					return;
				}
				x = Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile).x;
			}
			int index = GenDate.HourInt((long)Find.TickManager.TicksAbs, x);
			int num = GenDate.DayOfMonth((long)Find.TickManager.TicksAbs, x);
			Season season = GenDate.Season((long)Find.TickManager.TicksAbs, x);
			int num2 = GenDate.Year((long)Find.TickManager.TicksAbs, x);
			if (Mouse.IsOver(dateRect))
			{
				Widgets.DrawHighlight(dateRect);
			}
			GUI.BeginGroup(dateRect);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect = dateRect.AtZero();
			rect.xMax -= 7f;
			Widgets.Label(rect, DateReadout.fastHourStrings[index]);
			rect.yMin += 26f;
			if (num != DateReadout.dateStringDay || season != DateReadout.dateStringSeason || num2 != DateReadout.dateStringYear)
			{
				DateReadout.dateString = GenDate.DateReadoutStringAt((long)Find.TickManager.TicksAbs, x);
				DateReadout.dateStringDay = num;
				DateReadout.dateStringSeason = season;
				DateReadout.dateStringYear = num2;
			}
			Widgets.Label(rect, DateReadout.dateString);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			TooltipHandler.TipRegion(dateRect, new TipSignal(() => "DateReadoutTip".Translate(new object[]
			{
				GenDate.DaysPassed,
				15,
				season.Label()
			}), 86423));
		}
	}
}
