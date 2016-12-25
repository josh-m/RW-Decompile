using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainTabWindow_Restrict : MainTabWindow_PawnList
	{
		private const float TopAreaHeight = 65f;

		private const float TimeTablesWidth = 500f;

		private const float AAGapWidth = 6f;

		private TimeAssignmentDef selectedAssignment = TimeAssignmentDefOf.Work;

		private List<TimeAssignmentDef> clipboard;

		private float hourWidth;

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(1010f, 65f + (float)base.PawnsCount * 30f + 65f);
			}
		}

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			Rect position = new Rect(0f, 0f, fillRect.width, 65f);
			GUI.BeginGroup(position);
			Rect rect = new Rect(0f, 0f, 191f, position.height);
			this.DrawTimeAssignmentSelectorGrid(rect);
			float num = 201f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerLeft;
			for (int i = 0; i < 24; i++)
			{
				Rect rect2 = new Rect(num + 4f, 0f, this.hourWidth, position.height + 3f);
				Widgets.Label(rect2, i.ToString());
				num += this.hourWidth;
			}
			num += 6f;
			Rect rect3 = new Rect(num, 0f, position.width - num, Mathf.Round(position.height / 2f));
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(rect3, "ManageAreas".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageAreas(Find.VisibleMap));
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect4 = new Rect(num, 0f, position.width - num, position.height + 3f);
			Widgets.Label(rect4, "AllowedArea".Translate());
			GUI.EndGroup();
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			Rect rect5 = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
			base.DrawRows(rect5);
		}

		private void DrawTimeAssignmentSelectorGrid(Rect rect)
		{
			rect.yMax -= 2f;
			Rect rect2 = rect;
			rect2.xMax = rect2.center.x;
			rect2.yMax = rect2.center.y;
			this.DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Anything);
			rect2.x += rect2.width;
			this.DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Work);
			rect2.y += rect2.height;
			rect2.x -= rect2.width;
			this.DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Joy);
			rect2.x += rect2.width;
			this.DrawTimeAssignmentSelectorFor(rect2, TimeAssignmentDefOf.Sleep);
		}

		private void DrawTimeAssignmentSelectorFor(Rect rect, TimeAssignmentDef ta)
		{
			rect = rect.ContractedBy(2f);
			GUI.DrawTexture(rect, ta.ColorTexture);
			if (Widgets.ButtonInvisible(rect, false))
			{
				this.selectedAssignment = ta;
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
			}
			GUI.color = Color.white;
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = Color.white;
			Widgets.Label(rect, ta.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			if (this.selectedAssignment == ta)
			{
				Widgets.DrawBox(rect, 2);
			}
		}

		protected override void DrawPawnRow(Rect rect, Pawn p)
		{
			GUI.BeginGroup(rect);
			Action pasteAction = null;
			if (this.clipboard != null)
			{
				pasteAction = delegate
				{
					this.PasteTo(p);
				};
			}
			Rect rect2 = new Rect(165f, 0f, 36f, rect.height);
			CopyPasteUI.DoCopyPasteButtons(rect2, delegate
			{
				this.CopyFrom(p);
			}, pasteAction);
			float num = 165f + rect2.width;
			this.hourWidth = 20.833334f;
			for (int i = 0; i < 24; i++)
			{
				Rect rect3 = new Rect(num, 0f, this.hourWidth, rect.height);
				this.DoTimeAssignment(rect3, p, i);
				num += this.hourWidth;
			}
			GUI.color = Color.white;
			num += 6f;
			Rect rect4 = new Rect(num, 0f, rect.width - num, rect.height);
			AreaAllowedGUI.DoAllowedAreaSelectors(rect4, p, AllowedAreaMode.Humanlike);
			GUI.EndGroup();
		}

		private void DoTimeAssignment(Rect rect, Pawn p, int hour)
		{
			rect = rect.ContractedBy(1f);
			TimeAssignmentDef assignment = p.timetable.GetAssignment(hour);
			GUI.DrawTexture(rect, assignment.ColorTexture);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawBox(rect, 2);
				if (assignment != this.selectedAssignment && Input.GetMouseButton(0))
				{
					SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
					p.timetable.SetAssignment(hour, this.selectedAssignment);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeAssignments, KnowledgeAmount.SmallInteraction);
				}
			}
		}

		private void CopyFrom(Pawn p)
		{
			this.clipboard = p.timetable.times.ToList<TimeAssignmentDef>();
		}

		private void PasteTo(Pawn p)
		{
			for (int i = 0; i < 24; i++)
			{
				p.timetable.times[i] = this.clipboard[i];
			}
		}
	}
}
