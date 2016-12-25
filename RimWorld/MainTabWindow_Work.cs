using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Work : MainTabWindow_PawnList
	{
		private const float TopAreaHeight = 40f;

		protected const float LabelRowHeight = 50f;

		private const float LeftColumnWidth = 201f;

		private float workColumnSpacing = -1f;

		private static List<WorkTypeDef> visibleWorkTypesInPriorityOrder;

		private static DefMap<WorkTypeDef, Vector2> cachedLabelSizes = new DefMap<WorkTypeDef, Vector2>();

		private static DefMap<WorkTypeDef, int> clipboard;

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(1010f, 90f + (float)base.PawnsCount * 30f + 65f);
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			MainTabWindow_Work.visibleWorkTypesInPriorityOrder = (from def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
			where def.visible
			select def).ToList<WorkTypeDef>();
			foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
			{
				MainTabWindow_Work.cachedLabelSizes[current] = Text.CalcSize(current.labelShort);
			}
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DoWindowContents(rect);
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			Rect position = new Rect(0f, 0f, rect.width, 40f);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(5f, 5f, 140f, 30f);
			bool useWorkPriorities = Current.Game.playSettings.useWorkPriorities;
			Widgets.CheckboxLabeled(rect2, "ManualPriorities".Translate(), ref Current.Game.playSettings.useWorkPriorities, false);
			if (useWorkPriorities != Current.Game.playSettings.useWorkPriorities)
			{
				foreach (Pawn current in PawnsFinder.AllMapsAndWorld_Alive)
				{
					if (current.Faction == Faction.OfPlayer && current.workSettings != null)
					{
						current.workSettings.Notify_UseWorkPrioritiesChanged();
					}
				}
			}
			if (!Current.Game.playSettings.useWorkPriorities)
			{
				UIHighlighter.HighlightOpportunity(rect2, "ManualPriorities-Off");
			}
			float num = position.width / 3f;
			float num2 = position.width * 2f / 3f;
			Rect rect3 = new Rect(num - 50f, 5f, 160f, 30f);
			Rect rect4 = new Rect(num2 - 50f, 5f, 160f, 30f);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect3, "<= " + "HigherPriority".Translate());
			Widgets.Label(rect4, "LowerPriority".Translate() + " =>");
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			Rect position2 = new Rect(0f, 40f, rect.width, rect.height - 40f);
			GUI.BeginGroup(position2);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect rect5 = new Rect(0f, 50f, position2.width, position2.height - 50f);
			this.workColumnSpacing = (position2.width - 16f - 201f) / (float)MainTabWindow_Work.visibleWorkTypesInPriorityOrder.Count;
			float num3 = 201f;
			for (int i = 0; i < MainTabWindow_Work.visibleWorkTypesInPriorityOrder.Count; i++)
			{
				WorkTypeDef wdef = MainTabWindow_Work.visibleWorkTypesInPriorityOrder[i];
				Vector2 vector = MainTabWindow_Work.cachedLabelSizes[wdef];
				float num4 = num3 + 15f;
				Rect rect6 = new Rect(num4 - vector.x / 2f, 0f, vector.x, vector.y);
				if (i % 2 == 1)
				{
					rect6.y += 20f;
				}
				if (Mouse.IsOver(rect6))
				{
					Widgets.DrawHighlight(rect6);
				}
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect6, wdef.labelShort);
				TooltipHandler.TipRegion(rect6, new TipSignal(() => string.Concat(new string[]
				{
					wdef.gerundLabel,
					"\n\n",
					wdef.description,
					"\n\n",
					MainTabWindow_Work.SpecificWorkListString(wdef)
				}), wdef.GetHashCode()));
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
				Widgets.DrawLineVertical(num4, rect6.yMax - 3f, 50f - rect6.yMax + 3f);
				Widgets.DrawLineVertical(num4 + 1f, rect6.yMax - 3f, 50f - rect6.yMax + 3f);
				GUI.color = Color.white;
				num3 += this.workColumnSpacing;
			}
			base.DrawRows(rect5);
			GUI.EndGroup();
		}

		private static string SpecificWorkListString(WorkTypeDef def)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < def.workGiversByPriority.Count; i++)
			{
				stringBuilder.Append(def.workGiversByPriority[i].LabelCap);
				if (def.workGiversByPriority[i].emergency)
				{
					stringBuilder.Append(" (" + "EmergencyWorkMarker".Translate() + ")");
				}
				if (i < def.workGiversByPriority.Count - 1)
				{
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		protected override void DrawPawnRow(Rect rect, Pawn p)
		{
			float num = 165f;
			Action pasteAction = null;
			if (MainTabWindow_Work.clipboard != null)
			{
				pasteAction = delegate
				{
					MainTabWindow_Work.PasteTo(p);
				};
			}
			Rect rect2 = new Rect(num, rect.y, 36f, rect.height);
			CopyPasteUI.DoCopyPasteButtons(rect2, delegate
			{
				MainTabWindow_Work.CopyFrom(p);
			}, pasteAction);
			num = rect2.xMax;
			Text.Font = GameFont.Medium;
			float y = rect.y + 2.5f;
			for (int i = 0; i < MainTabWindow_Work.visibleWorkTypesInPriorityOrder.Count; i++)
			{
				WorkTypeDef wdef = MainTabWindow_Work.visibleWorkTypesInPriorityOrder[i];
				bool incapable = this.IsIncapableOfWholeWorkType(p, MainTabWindow_Work.visibleWorkTypesInPriorityOrder[i]);
				WidgetsWork.DrawWorkBoxFor(num, y, p, wdef, incapable);
				Rect rect3 = new Rect(num, y, 25f, 25f);
				TooltipHandler.TipRegion(rect3, () => WidgetsWork.TipForPawnWorker(p, wdef, incapable), p.thingIDNumber ^ wdef.GetHashCode());
				num += this.workColumnSpacing;
			}
		}

		private bool IsIncapableOfWholeWorkType(Pawn p, WorkTypeDef work)
		{
			for (int i = 0; i < work.workGiversByPriority.Count; i++)
			{
				bool flag = true;
				for (int j = 0; j < work.workGiversByPriority[i].requiredCapacities.Count; j++)
				{
					PawnCapacityDef capacity = work.workGiversByPriority[i].requiredCapacities[j];
					if (!p.health.capacities.CapableOf(capacity))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		private static void CopyFrom(Pawn p)
		{
			if (MainTabWindow_Work.clipboard == null)
			{
				MainTabWindow_Work.clipboard = new DefMap<WorkTypeDef, int>();
			}
			foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
			{
				MainTabWindow_Work.clipboard[current] = (p.story.WorkTypeIsDisabled(current) ? 3 : p.workSettings.GetPriority(current));
			}
		}

		private static void PasteTo(Pawn p)
		{
			foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if (!p.story.WorkTypeIsDisabled(current))
				{
					p.workSettings.SetPriority(current, MainTabWindow_Work.clipboard[current]);
				}
			}
		}
	}
}
