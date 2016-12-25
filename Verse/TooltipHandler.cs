using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class TooltipHandler
	{
		private const float SpaceBetweenTooltips = 2f;

		private static Dictionary<int, ActiveTip> activeTips = new Dictionary<int, ActiveTip>();

		private static int frame = 0;

		private static List<int> dyingTips = new List<int>(32);

		private static float TooltipDelay = 0.45f;

		public static void ClearTooltipsFrom(Rect rect)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.dyingTips.Clear();
				foreach (KeyValuePair<int, ActiveTip> current in TooltipHandler.activeTips)
				{
					if (current.Value.lastTriggerFrame == TooltipHandler.frame)
					{
						TooltipHandler.dyingTips.Add(current.Key);
					}
				}
				for (int i = 0; i < TooltipHandler.dyingTips.Count; i++)
				{
					TooltipHandler.activeTips.Remove(TooltipHandler.dyingTips[i]);
				}
			}
		}

		public static void TipRegion(Rect rect, Func<string> textGetter, int uniqueId)
		{
			TooltipHandler.TipRegion(rect, new TipSignal(textGetter, uniqueId));
		}

		public static void TipRegion(Rect rect, TipSignal tip)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (!Mouse.IsOver(rect))
			{
				return;
			}
			if (DebugViewSettings.drawTooltipEdges)
			{
				Widgets.DrawBox(rect, 1);
			}
			if (!TooltipHandler.activeTips.ContainsKey(tip.uniqueId))
			{
				ActiveTip value = new ActiveTip(tip);
				TooltipHandler.activeTips.Add(tip.uniqueId, value);
				TooltipHandler.activeTips[tip.uniqueId].firstTriggerTime = (double)Time.realtimeSinceStartup;
			}
			TooltipHandler.activeTips[tip.uniqueId].lastTriggerFrame = TooltipHandler.frame;
			TooltipHandler.activeTips[tip.uniqueId].signal.text = tip.text;
			TooltipHandler.activeTips[tip.uniqueId].signal.textGetter = tip.textGetter;
		}

		public static void DoTooltipGUI()
		{
			TooltipHandler.DrawActiveTips();
			if (Event.current.type == EventType.Repaint)
			{
				TooltipHandler.CleanActiveTooltips();
				TooltipHandler.frame++;
			}
		}

		private static void DrawActiveTips()
		{
			List<ActiveTip> list = new List<ActiveTip>();
			foreach (KeyValuePair<int, ActiveTip> current in TooltipHandler.activeTips)
			{
				if ((double)Time.realtimeSinceStartup > current.Value.firstTriggerTime + (double)TooltipHandler.TooltipDelay)
				{
					list.Add(current.Value);
				}
			}
			list.Sort(new Comparison<ActiveTip>(TooltipHandler.CompareTooltipsByPriority));
			Vector2 pos = TooltipHandler.CalculateInitialTipPosition(list);
			for (int i = 0; i < list.Count; i++)
			{
				pos.y += list[i].DrawTooltip(pos);
				pos.y += 2f;
			}
		}

		private static void CleanActiveTooltips()
		{
			TooltipHandler.dyingTips.Clear();
			foreach (KeyValuePair<int, ActiveTip> current in TooltipHandler.activeTips)
			{
				if (current.Value.lastTriggerFrame != TooltipHandler.frame)
				{
					TooltipHandler.dyingTips.Add(current.Key);
				}
			}
			for (int i = 0; i < TooltipHandler.dyingTips.Count; i++)
			{
				TooltipHandler.activeTips.Remove(TooltipHandler.dyingTips[i]);
			}
		}

		private static Vector2 CalculateInitialTipPosition(List<ActiveTip> drawingTips)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < drawingTips.Count; i++)
			{
				Rect tipRect = drawingTips[i].TipRect;
				num += tipRect.height;
				num2 = Mathf.Max(num2, tipRect.width);
				if (i != drawingTips.Count - 1)
				{
					num += 2f;
				}
			}
			Vector3 vector = Event.current.mousePosition;
			float y;
			if (vector.y + 14f + num < (float)UI.screenHeight)
			{
				y = vector.y + 14f;
			}
			else if (vector.y - 5f - num >= 0f)
			{
				y = vector.y - 5f - num;
			}
			else
			{
				y = 0f;
			}
			float x;
			if (vector.x + 16f + num2 < (float)UI.screenWidth)
			{
				x = vector.x + 16f;
			}
			else
			{
				x = vector.x - 4f - num2;
			}
			return new Vector2(x, y);
		}

		private static int CompareTooltipsByPriority(ActiveTip A, ActiveTip B)
		{
			if (A.signal.priority == B.signal.priority)
			{
				return 0;
			}
			if (A.signal.priority == TooltipPriority.Pawn)
			{
				return -1;
			}
			if (B.signal.priority == TooltipPriority.Pawn)
			{
				return 1;
			}
			return 0;
		}
	}
}
