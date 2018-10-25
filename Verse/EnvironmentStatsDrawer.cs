using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class EnvironmentStatsDrawer
	{
		private const float StatLabelColumnWidth = 100f;

		private const float ScoreColumnWidth = 50f;

		private const float ScoreStageLabelColumnWidth = 160f;

		private static readonly Color RelatedStatColor = new Color(0.85f, 0.85f, 0.85f);

		private const float DistFromMouse = 26f;

		private const float WindowPadding = 18f;

		private const float LineHeight = 23f;

		private const float SpaceBetweenLines = 2f;

		private const float SpaceBetweenColumns = 35f;

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

		private static bool ShouldShowWindowNow()
		{
			return (EnvironmentStatsDrawer.ShouldShowRoomStats() || EnvironmentStatsDrawer.ShouldShowBeauty()) && !Mouse.IsInputBlockedNow;
		}

		private static bool ShouldShowRoomStats()
		{
			if (!Find.PlaySettings.showRoomStats)
			{
				return false;
			}
			if (!UI.MouseCell().InBounds(Find.CurrentMap) || UI.MouseCell().Fogged(Find.CurrentMap))
			{
				return false;
			}
			Room room = UI.MouseCell().GetRoom(Find.CurrentMap, RegionType.Set_All);
			return room != null && room.Role != RoomRoleDefOf.None;
		}

		private static bool ShouldShowBeauty()
		{
			return Find.PlaySettings.showBeauty && UI.MouseCell().InBounds(Find.CurrentMap) && !UI.MouseCell().Fogged(Find.CurrentMap) && UI.MouseCell().GetRoom(Find.CurrentMap, RegionType.Set_Passable) != null;
		}

		public static void EnvironmentStatsOnGUI()
		{
			if (Event.current.type != EventType.Repaint || !EnvironmentStatsDrawer.ShouldShowWindowNow())
			{
				return;
			}
			EnvironmentStatsDrawer.DrawInfoWindow();
		}

		private static void DrawInfoWindow()
		{
			EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 <DrawInfoWindow>c__AnonStorey = new EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0();
			Text.Font = GameFont.Small;
			<DrawInfoWindow>c__AnonStorey.windowRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 416f, 36f);
			bool flag = EnvironmentStatsDrawer.ShouldShowBeauty();
			if (flag)
			{
				EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_57_cp_0 = <DrawInfoWindow>c__AnonStorey;
				expr_57_cp_0.windowRect.height = expr_57_cp_0.windowRect.height + 25f;
			}
			if (EnvironmentStatsDrawer.ShouldShowRoomStats())
			{
				if (flag)
				{
					EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_7E_cp_0 = <DrawInfoWindow>c__AnonStorey;
					expr_7E_cp_0.windowRect.height = expr_7E_cp_0.windowRect.height + 13f;
				}
				EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_95_cp_0 = <DrawInfoWindow>c__AnonStorey;
				expr_95_cp_0.windowRect.height = expr_95_cp_0.windowRect.height + 23f;
				EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_AC_cp_0 = <DrawInfoWindow>c__AnonStorey;
				expr_AC_cp_0.windowRect.height = expr_AC_cp_0.windowRect.height + (float)EnvironmentStatsDrawer.DisplayedRoomStatsCount * 25f;
			}
			EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_CA_cp_0 = <DrawInfoWindow>c__AnonStorey;
			expr_CA_cp_0.windowRect.x = expr_CA_cp_0.windowRect.x + 26f;
			EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_E1_cp_0 = <DrawInfoWindow>c__AnonStorey;
			expr_E1_cp_0.windowRect.y = expr_E1_cp_0.windowRect.y + 26f;
			if (<DrawInfoWindow>c__AnonStorey.windowRect.xMax > (float)UI.screenWidth)
			{
				EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_10E_cp_0 = <DrawInfoWindow>c__AnonStorey;
				expr_10E_cp_0.windowRect.x = expr_10E_cp_0.windowRect.x - (<DrawInfoWindow>c__AnonStorey.windowRect.width + 52f);
			}
			if (<DrawInfoWindow>c__AnonStorey.windowRect.yMax > (float)UI.screenHeight)
			{
				EnvironmentStatsDrawer.<DrawInfoWindow>c__AnonStorey0 expr_147_cp_0 = <DrawInfoWindow>c__AnonStorey;
				expr_147_cp_0.windowRect.y = expr_147_cp_0.windowRect.y - (<DrawInfoWindow>c__AnonStorey.windowRect.height + 52f);
			}
			Find.WindowStack.ImmediateWindow(74975, <DrawInfoWindow>c__AnonStorey.windowRect, WindowLayer.Super, delegate
			{
				EnvironmentStatsDrawer.FillWindow(<DrawInfoWindow>c__AnonStorey.windowRect);
			}, true, false, 1f);
		}

		private static void FillWindow(Rect windowRect)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InspectRoomStats, KnowledgeAmount.FrameDisplayed);
			Text.Font = GameFont.Small;
			float num = 18f;
			bool flag = EnvironmentStatsDrawer.ShouldShowBeauty();
			if (flag)
			{
				float beauty = BeautyUtility.AverageBeautyPerceptible(UI.MouseCell(), Find.CurrentMap);
				Rect rect = new Rect(18f, num, windowRect.width - 36f, 100f);
				GUI.color = BeautyDrawer.BeautyColor(beauty, 40f);
				Widgets.Label(rect, "BeautyHere".Translate() + ": " + beauty.ToString("F1"));
				num += 25f;
			}
			if (EnvironmentStatsDrawer.ShouldShowRoomStats())
			{
				if (flag)
				{
					num += 5f;
					GUI.color = new Color(1f, 1f, 1f, 0.4f);
					Widgets.DrawLineHorizontal(18f, num, windowRect.width - 36f);
					GUI.color = Color.white;
					num += 8f;
				}
				Room room = UI.MouseCell().GetRoom(Find.CurrentMap, RegionType.Set_All);
				Rect rect2 = new Rect(18f, num, windowRect.width - 36f, 100f);
				GUI.color = Color.white;
				Widgets.Label(rect2, EnvironmentStatsDrawer.GetRoomRoleLabel(room));
				num += 25f;
				Text.WordWrap = false;
				for (int i = 0; i < DefDatabase<RoomStatDef>.AllDefsListForReading.Count; i++)
				{
					RoomStatDef roomStatDef = DefDatabase<RoomStatDef>.AllDefsListForReading[i];
					if (!roomStatDef.isHidden || DebugViewSettings.showAllRoomStats)
					{
						float stat = room.GetStat(roomStatDef);
						RoomStatScoreStage scoreStage = roomStatDef.GetScoreStage(stat);
						if (room.Role.IsStatRelated(roomStatDef))
						{
							GUI.color = EnvironmentStatsDrawer.RelatedStatColor;
						}
						else
						{
							GUI.color = Color.gray;
						}
						Rect rect3 = new Rect(rect2.x, num, 100f, 23f);
						Widgets.Label(rect3, roomStatDef.LabelCap);
						Rect rect4 = new Rect(rect3.xMax + 35f, num, 50f, 23f);
						string label = roomStatDef.ScoreToString(stat);
						Widgets.Label(rect4, label);
						Rect rect5 = new Rect(rect4.xMax + 35f, num, 160f, 23f);
						Widgets.Label(rect5, (scoreStage != null) ? scoreStage.label : string.Empty);
						num += 25f;
					}
				}
				Text.WordWrap = true;
			}
			GUI.color = Color.white;
		}

		public static void DrawRoomOverlays()
		{
			if (Find.PlaySettings.showBeauty && UI.MouseCell().InBounds(Find.CurrentMap))
			{
				GenUI.RenderMouseoverBracket();
			}
			if (EnvironmentStatsDrawer.ShouldShowWindowNow() && EnvironmentStatsDrawer.ShouldShowRoomStats())
			{
				Room room = UI.MouseCell().GetRoom(Find.CurrentMap, RegionType.Set_All);
				if (room != null && room.Role != RoomRoleDefOf.None)
				{
					room.DrawFieldEdges();
				}
			}
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
				result = "SomeonesRoom".Translate(pawn.LabelShort, room.Role.label, pawn.Named("PAWN"));
			}
			else
			{
				result = "CouplesRoom".Translate(pawn.LabelShort, pawn2.LabelShort, room.Role.label, pawn.Named("PAWN1"), pawn2.Named("PAWN2"));
			}
			return result;
		}
	}
}
