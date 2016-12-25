using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal static class RoomStatsDrawer
	{
		private const int WindowPadding = 19;

		private const int LineHeight = 23;

		private const int SpaceBetweenLines = 2;

		private const float SpaceBetweenColumns = 35f;

		private static float statLabelColumnWidth = 0f;

		private static float scoreColumnWidth = 0f;

		private static float scoreStageLabelColumnWidth = 0f;

		private static readonly Color RelatedStatColor = new Color(0.85f, 0.85f, 0.85f);

		private static bool ShouldShowRoomStats
		{
			get
			{
				if (!Find.PlaySettings.showRoomStats)
				{
					return false;
				}
				if (Mouse.IsInputBlockedNow)
				{
					return false;
				}
				if (!Gen.MouseCell().InBounds() || Gen.MouseCell().Fogged())
				{
					return false;
				}
				Room room = Gen.MouseCell().GetRoom();
				return room != null && room.Role != RoomRoleDefOf.None;
			}
		}

		private static int DisplayedRoomStatsCount
		{
			get
			{
				int num = 0;
				List<RoomStatDef> allDefsListForReading = DefDatabase<RoomStatDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (!allDefsListForReading[i].isHidden || DebugViewSettings.showAllRoomStats)
					{
						num++;
					}
				}
				return num;
			}
		}

		public static void RoomStatsOnGUI()
		{
			RoomStatsDrawer.<RoomStatsOnGUI>c__AnonStorey471 <RoomStatsOnGUI>c__AnonStorey = new RoomStatsDrawer.<RoomStatsOnGUI>c__AnonStorey471();
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (!RoomStatsDrawer.ShouldShowRoomStats)
			{
				return;
			}
			<RoomStatsOnGUI>c__AnonStorey.room = Gen.MouseCell().GetRoom();
			Text.Font = GameFont.Small;
			RoomStatsDrawer.CalculateColumnsSizes(<RoomStatsOnGUI>c__AnonStorey.room);
			<RoomStatsOnGUI>c__AnonStorey.windowRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 108f + RoomStatsDrawer.statLabelColumnWidth + RoomStatsDrawer.scoreColumnWidth + RoomStatsDrawer.scoreStageLabelColumnWidth, (float)(65 + RoomStatsDrawer.DisplayedRoomStatsCount * 25));
			RoomStatsDrawer.<RoomStatsOnGUI>c__AnonStorey471 expr_9B_cp_0 = <RoomStatsOnGUI>c__AnonStorey;
			expr_9B_cp_0.windowRect.x = expr_9B_cp_0.windowRect.x + 5f;
			RoomStatsDrawer.<RoomStatsOnGUI>c__AnonStorey471 expr_B2_cp_0 = <RoomStatsOnGUI>c__AnonStorey;
			expr_B2_cp_0.windowRect.y = expr_B2_cp_0.windowRect.y + 5f;
			if (<RoomStatsOnGUI>c__AnonStorey.windowRect.xMax > (float)Screen.width)
			{
				RoomStatsDrawer.<RoomStatsOnGUI>c__AnonStorey471 expr_DF_cp_0 = <RoomStatsOnGUI>c__AnonStorey;
				expr_DF_cp_0.windowRect.x = expr_DF_cp_0.windowRect.x - (<RoomStatsOnGUI>c__AnonStorey.windowRect.width + 10f);
			}
			if (<RoomStatsOnGUI>c__AnonStorey.windowRect.yMax > (float)Screen.height)
			{
				RoomStatsDrawer.<RoomStatsOnGUI>c__AnonStorey471 expr_118_cp_0 = <RoomStatsOnGUI>c__AnonStorey;
				expr_118_cp_0.windowRect.y = expr_118_cp_0.windowRect.y - (<RoomStatsOnGUI>c__AnonStorey.windowRect.height + 10f);
			}
			Find.WindowStack.ImmediateWindow(74975, <RoomStatsOnGUI>c__AnonStorey.windowRect, WindowLayer.Super, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InspectRoomStats, KnowledgeAmount.FrameDisplayed);
				Text.Font = GameFont.Small;
				float num = 19f;
				Rect rect = new Rect(19f, num, <RoomStatsOnGUI>c__AnonStorey.windowRect.width - 38f, 100f);
				GUI.color = Color.white;
				Widgets.Label(rect, RoomStatsDrawer.GetRoomRoleLabel(<RoomStatsOnGUI>c__AnonStorey.room));
				num += 25f;
				for (int i = 0; i < DefDatabase<RoomStatDef>.AllDefsListForReading.Count; i++)
				{
					RoomStatDef roomStatDef = DefDatabase<RoomStatDef>.AllDefsListForReading[i];
					if (!roomStatDef.isHidden || DebugViewSettings.showAllRoomStats)
					{
						float stat = <RoomStatsOnGUI>c__AnonStorey.room.GetStat(roomStatDef);
						RoomStatScoreStage scoreStage = roomStatDef.GetScoreStage(stat);
						if (<RoomStatsOnGUI>c__AnonStorey.room.Role.IsStatRelated(roomStatDef))
						{
							GUI.color = RoomStatsDrawer.RelatedStatColor;
						}
						else
						{
							GUI.color = Color.gray;
						}
						Rect rect2 = new Rect(rect.x, num, RoomStatsDrawer.statLabelColumnWidth, 23f);
						Widgets.Label(rect2, roomStatDef.LabelCap);
						Rect rect3 = new Rect(rect2.xMax + 35f, num, RoomStatsDrawer.scoreColumnWidth, 23f);
						string label;
						if (roomStatDef.displayRounded)
						{
							label = Mathf.RoundToInt(stat).ToString();
						}
						else
						{
							label = stat.ToString("0.##");
						}
						Widgets.Label(rect3, label);
						Rect rect4 = new Rect(rect3.xMax + 35f, num, RoomStatsDrawer.scoreStageLabelColumnWidth, 23f);
						Widgets.Label(rect4, (scoreStage != null) ? scoreStage.label : string.Empty);
						num += 25f;
					}
				}
				GUI.color = Color.white;
			}, true, false, 1f);
		}

		public static void DrawRoomOverlays()
		{
			if (!RoomStatsDrawer.ShouldShowRoomStats)
			{
				return;
			}
			Room room = Gen.MouseCell().GetRoom();
			room.DrawFieldEdges();
		}

		private static string GetRoomRoleLabel(Room room)
		{
			Pawn pawn = null;
			Pawn pawn2 = null;
			foreach (Pawn current in room.Owners)
			{
				if (pawn == null)
				{
					pawn = current;
				}
				else
				{
					pawn2 = current;
				}
			}
			string result;
			if (pawn == null)
			{
				result = room.Role.LabelCap;
			}
			else if (pawn2 == null)
			{
				result = "SomeonesRoom".Translate(new object[]
				{
					pawn.NameStringShort,
					room.Role.label
				});
			}
			else
			{
				result = "CouplesRoom".Translate(new object[]
				{
					pawn.NameStringShort,
					pawn2.NameStringShort,
					room.Role.label
				});
			}
			return result;
		}

		private static void CalculateColumnsSizes(Room room)
		{
			RoomStatsDrawer.statLabelColumnWidth = 0f;
			RoomStatsDrawer.scoreColumnWidth = 0f;
			RoomStatsDrawer.scoreStageLabelColumnWidth = 0f;
			for (int i = 0; i < DefDatabase<RoomStatDef>.AllDefsListForReading.Count; i++)
			{
				RoomStatDef roomStatDef = DefDatabase<RoomStatDef>.AllDefsListForReading[i];
				if (!roomStatDef.isHidden || DebugViewSettings.showAllRoomStats)
				{
					RoomStatsDrawer.statLabelColumnWidth = Mathf.Max(RoomStatsDrawer.statLabelColumnWidth, Text.CalcSize(roomStatDef.LabelCap).x);
					float stat = room.GetStat(roomStatDef);
					RoomStatScoreStage scoreStage = roomStatDef.GetScoreStage(stat);
					string text = (scoreStage != null) ? scoreStage.label : string.Empty;
					RoomStatsDrawer.scoreStageLabelColumnWidth = Mathf.Max(RoomStatsDrawer.scoreStageLabelColumnWidth, Text.CalcSize(text).x);
					string text2;
					if (roomStatDef.displayRounded)
					{
						text2 = Mathf.RoundToInt(stat).ToString();
					}
					else
					{
						text2 = stat.ToString("0.##");
					}
					RoomStatsDrawer.scoreColumnWidth = Mathf.Max(RoomStatsDrawer.scoreColumnWidth, Text.CalcSize(text2).x);
				}
			}
			RoomStatsDrawer.scoreColumnWidth = Mathf.Max(RoomStatsDrawer.scoreColumnWidth, 40f);
		}
	}
}
