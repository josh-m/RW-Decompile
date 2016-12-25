using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class ReorderableWidget
	{
		private struct ReorderableGroup
		{
			public Action<int, int> reorderedAction;
		}

		private struct ReorderableInstance
		{
			public int groupID;

			public Rect rect;

			public Rect absRect;
		}

		private const float MinMouseMoveToHighlightReorderable = 5f;

		private const float LineWidth = 2f;

		private static List<ReorderableWidget.ReorderableGroup> groups = new List<ReorderableWidget.ReorderableGroup>();

		private static List<ReorderableWidget.ReorderableInstance> reorderables = new List<ReorderableWidget.ReorderableInstance>();

		private static int draggingReorderable = -1;

		private static bool clicked;

		private static bool released;

		private static Vector2 clickedAt;

		private static Rect clickedInRect;

		private static int lastInsertAt = -1;

		private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.3f);

		private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.3f);

		public static void ReorderableWidgetOnGUI()
		{
			if (Event.current.rawType == EventType.MouseUp)
			{
				ReorderableWidget.released = true;
			}
			if (Event.current.type == EventType.Repaint)
			{
				if (ReorderableWidget.clicked)
				{
					ReorderableWidget.draggingReorderable = -1;
					for (int i = 0; i < ReorderableWidget.reorderables.Count; i++)
					{
						if (ReorderableWidget.reorderables[i].rect == ReorderableWidget.clickedInRect)
						{
							ReorderableWidget.draggingReorderable = i;
							break;
						}
					}
					ReorderableWidget.clicked = false;
				}
				if (ReorderableWidget.draggingReorderable >= ReorderableWidget.reorderables.Count)
				{
					ReorderableWidget.draggingReorderable = -1;
				}
				ReorderableWidget.lastInsertAt = ReorderableWidget.CurrentInsertAt();
				if (ReorderableWidget.released)
				{
					ReorderableWidget.released = false;
					if (ReorderableWidget.lastInsertAt >= 0 && ReorderableWidget.lastInsertAt != ReorderableWidget.draggingReorderable)
					{
						ReorderableWidget.groups[ReorderableWidget.reorderables[ReorderableWidget.draggingReorderable].groupID].reorderedAction(ReorderableWidget.draggingReorderable, ReorderableWidget.lastInsertAt);
					}
					ReorderableWidget.draggingReorderable = -1;
					ReorderableWidget.lastInsertAt = -1;
				}
				ReorderableWidget.groups.Clear();
				ReorderableWidget.reorderables.Clear();
			}
		}

		public static int NewGroup(Action<int, int> reorderedAction)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return -1;
			}
			ReorderableWidget.ReorderableGroup item = default(ReorderableWidget.ReorderableGroup);
			item.reorderedAction = reorderedAction;
			ReorderableWidget.groups.Add(item);
			return ReorderableWidget.groups.Count - 1;
		}

		public static void Reorderable(int groupID, Rect rect)
		{
			if (Event.current.type == EventType.Repaint)
			{
				ReorderableWidget.ReorderableInstance item = default(ReorderableWidget.ReorderableInstance);
				item.groupID = groupID;
				item.rect = rect;
				item.absRect = new Rect(UI.GUIToScreenPoint(rect.position), rect.size);
				ReorderableWidget.reorderables.Add(item);
				int num = ReorderableWidget.reorderables.Count - 1;
				if (Vector2.Distance(ReorderableWidget.clickedAt, Event.current.mousePosition) > 5f)
				{
					if (ReorderableWidget.draggingReorderable == num)
					{
						GUI.color = ReorderableWidget.HighlightColor;
						Widgets.DrawHighlight(rect);
						GUI.color = Color.white;
					}
					if (ReorderableWidget.lastInsertAt == num)
					{
						Rect rect2 = ReorderableWidget.reorderables[ReorderableWidget.lastInsertAt].rect;
						bool flag = Event.current.mousePosition.y < rect2.center.y;
						GUI.color = ReorderableWidget.LineColor;
						if (flag)
						{
							Widgets.DrawLine(rect2.position, new Vector2(rect2.x + rect2.width, rect2.y), ReorderableWidget.LineColor, 2f);
						}
						else
						{
							Widgets.DrawLine(new Vector2(rect2.x, rect2.yMax), new Vector2(rect2.x + rect2.width, rect2.yMax), ReorderableWidget.LineColor, 2f);
						}
						GUI.color = Color.white;
					}
				}
			}
			if (Event.current.rawType == EventType.MouseUp)
			{
				ReorderableWidget.released = true;
			}
			if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
			{
				ReorderableWidget.clicked = true;
				ReorderableWidget.clickedAt = Event.current.mousePosition;
				ReorderableWidget.clickedInRect = rect;
			}
		}

		private static int CurrentInsertAt()
		{
			if (ReorderableWidget.draggingReorderable < 0)
			{
				return -1;
			}
			int groupID = ReorderableWidget.reorderables[ReorderableWidget.draggingReorderable].groupID;
			if (groupID < 0 || groupID >= ReorderableWidget.groups.Count)
			{
				Log.ErrorOnce("Reorderable used invalid group.", 1968375560);
				return -1;
			}
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < ReorderableWidget.reorderables.Count; i++)
			{
				ReorderableWidget.ReorderableInstance reorderableInstance = ReorderableWidget.reorderables[i];
				if (reorderableInstance.groupID == groupID)
				{
					int num3 = (i > ReorderableWidget.draggingReorderable) ? num2 : i;
					if (reorderableInstance.absRect.TopHalf().Contains(Event.current.mousePosition))
					{
						num = num3;
						break;
					}
					if (num2 >= 0)
					{
						float num4 = Mathf.Min(reorderableInstance.absRect.x, ReorderableWidget.reorderables[num2].absRect.x);
						Rect rect = new Rect(num4, ReorderableWidget.reorderables[num2].absRect.center.y, Mathf.Max(reorderableInstance.absRect.xMax, ReorderableWidget.reorderables[num2].absRect.xMax) - num4, reorderableInstance.absRect.center.y - ReorderableWidget.reorderables[num2].absRect.center.y);
						if (rect.Contains(Event.current.mousePosition))
						{
							num = num3;
							break;
						}
					}
					num2 = i;
				}
			}
			if (num < 0 && num2 >= 0 && ReorderableWidget.reorderables[num2].absRect.BottomHalf().Contains(Event.current.mousePosition))
			{
				num = num2;
			}
			return num;
		}
	}
}
