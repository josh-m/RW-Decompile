using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class RecordsCardUtility
	{
		private const float RecordsLeftPadding = 8f;

		private static Vector2 scrollPosition;

		private static float listHeight;

		public static void DrawRecordsCard(Rect rect, Pawn pawn)
		{
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f, RecordsCardUtility.listHeight);
			Widgets.BeginScrollView(rect, ref RecordsCardUtility.scrollPosition, rect2);
			Rect leftRect = rect2;
			leftRect.width *= 0.5f;
			Rect rightRect = rect2;
			rightRect.x = leftRect.xMax;
			rightRect.width = rect2.width - rightRect.x;
			leftRect.xMax -= 6f;
			rightRect.xMin += 6f;
			float a = RecordsCardUtility.DrawTimeRecords(leftRect, pawn);
			float b = RecordsCardUtility.DrawMiscRecords(rightRect, pawn);
			RecordsCardUtility.listHeight = Mathf.Max(a, b) + 100f;
			Widgets.EndScrollView();
		}

		private static float DrawTimeRecords(Rect leftRect, Pawn pawn)
		{
			List<RecordDef> allDefsListForReading = DefDatabase<RecordDef>.AllDefsListForReading;
			float num = 0f;
			GUI.BeginGroup(leftRect);
			Widgets.ListSeparator(ref num, leftRect.width, "TimeRecordsCategory".Translate());
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].type == RecordType.Time)
				{
					num += RecordsCardUtility.DrawRecord(8f, num, leftRect.width - 8f, allDefsListForReading[i], pawn);
				}
			}
			GUI.EndGroup();
			return num;
		}

		private static float DrawMiscRecords(Rect rightRect, Pawn pawn)
		{
			List<RecordDef> allDefsListForReading = DefDatabase<RecordDef>.AllDefsListForReading;
			float num = 0f;
			GUI.BeginGroup(rightRect);
			Widgets.ListSeparator(ref num, rightRect.width, "MiscRecordsCategory".Translate());
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].type == RecordType.Int || allDefsListForReading[i].type == RecordType.Float)
				{
					num += RecordsCardUtility.DrawRecord(8f, num, rightRect.width - 8f, allDefsListForReading[i], pawn);
				}
			}
			GUI.EndGroup();
			return num;
		}

		private static float DrawRecord(float x, float y, float width, RecordDef record, Pawn pawn)
		{
			float num = width * 0.45f;
			string text;
			if (record.type == RecordType.Time)
			{
				text = pawn.records.GetAsInt(record).ToStringTicksToPeriod(true);
			}
			else
			{
				text = pawn.records.GetValue(record).ToString("0.##");
			}
			Rect rect = new Rect(8f, y, width, Text.CalcHeight(text, num));
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			Rect rect2 = rect;
			rect2.width -= num;
			Widgets.Label(rect2, record.LabelCap);
			Rect rect3 = rect;
			rect3.x = rect2.xMax;
			rect3.width = num;
			Widgets.Label(rect3, text);
			TooltipHandler.TipRegion(rect, new TipSignal(() => record.description, record.GetHashCode()));
			return rect.height;
		}
	}
}
