using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class EditWindow_Log : EditWindow
	{
		private const float CountWidth = 28f;

		private const float Yinc = 25f;

		private const float DetailsPaneBorderHeight = 7f;

		private const float DetailsPaneMinHeight = 20f;

		private const float ListingMinHeight = 80f;

		private const float TopAreaHeight = 26f;

		private const float MessageMaxHeight = 30f;

		private static LogMessage selectedMessage = null;

		private static Vector2 messagesScrollPosition;

		private static Vector2 detailsScrollPosition;

		private float listingViewHeight;

		private static float detailsPaneHeight = 100f;

		private bool borderDragging;

		private static bool canAutoOpen = true;

		public static bool wantsToOpen = false;

		private static readonly Texture2D AltMessageTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.17f, 0.17f, 0.17f, 0.85f));

		private static readonly Texture2D SelectedMessageTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.25f, 0.25f, 0.17f, 0.85f));

		private static readonly Texture2D StackTraceAreaTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.5f));

		private static readonly Texture2D StackTraceBorderTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0.3f, 0.3f, 1f));

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2((float)UI.screenWidth / 2f, (float)UI.screenHeight / 2f);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public EditWindow_Log()
		{
			this.optionalTitle = "Debug log";
		}

		public static void TryAutoOpen()
		{
			if (EditWindow_Log.canAutoOpen)
			{
				EditWindow_Log.wantsToOpen = true;
			}
		}

		public static void ClearSelectedMessage()
		{
			EditWindow_Log.selectedMessage = null;
			EditWindow_Log.detailsScrollPosition = Vector2.zero;
		}

		public static void ClearAll()
		{
			EditWindow_Log.ClearSelectedMessage();
			EditWindow_Log.messagesScrollPosition = Vector2.zero;
		}

		public override void PostClose()
		{
			base.PostClose();
			EditWindow_Log.wantsToOpen = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Tiny;
			WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
			if (widgetRow.ButtonText("Clear", "Clear all log messages.", true, false))
			{
				Log.Clear();
				EditWindow_Log.ClearAll();
			}
			if (widgetRow.ButtonText("Trace big", "Set the stack trace to be large on screen.", true, false))
			{
				EditWindow_Log.detailsPaneHeight = 700f;
			}
			if (widgetRow.ButtonText("Trace medium", "Set the stack trace to be medium-sized on screen.", true, false))
			{
				EditWindow_Log.detailsPaneHeight = 300f;
			}
			if (widgetRow.ButtonText("Trace small", "Set the stack trace to be small on screen.", true, false))
			{
				EditWindow_Log.detailsPaneHeight = 100f;
			}
			if (EditWindow_Log.canAutoOpen)
			{
				if (widgetRow.ButtonText("Auto-open is ON", string.Empty, true, false))
				{
					EditWindow_Log.canAutoOpen = false;
				}
			}
			else if (widgetRow.ButtonText("Auto-open is OFF", string.Empty, true, false))
			{
				EditWindow_Log.canAutoOpen = true;
			}
			Text.Font = GameFont.Small;
			Rect rect = new Rect(inRect);
			rect.yMin += 26f;
			rect.yMax = inRect.height;
			if (EditWindow_Log.selectedMessage != null)
			{
				rect.yMax -= EditWindow_Log.detailsPaneHeight;
			}
			Rect detailsRect = new Rect(inRect);
			detailsRect.yMin = rect.yMax;
			this.DoMessagesListing(rect);
			this.DoMessageDetails(detailsRect, inRect);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect))
			{
				EditWindow_Log.ClearSelectedMessage();
			}
			EditWindow_Log.detailsPaneHeight = Mathf.Max(EditWindow_Log.detailsPaneHeight, 20f);
			EditWindow_Log.detailsPaneHeight = Mathf.Min(EditWindow_Log.detailsPaneHeight, inRect.height - 80f);
		}

		public static void Notify_MessageDequeued(LogMessage oldMessage)
		{
			if (EditWindow_Log.selectedMessage == oldMessage)
			{
				EditWindow_Log.selectedMessage = null;
			}
		}

		private void DoMessagesListing(Rect listingRect)
		{
			Rect viewRect = new Rect(0f, 0f, listingRect.width - 16f, this.listingViewHeight + 100f);
			Widgets.BeginScrollView(listingRect, ref EditWindow_Log.messagesScrollPosition, viewRect);
			float width = viewRect.width - 28f;
			Text.Font = GameFont.Tiny;
			float num = 0f;
			bool flag = false;
			foreach (LogMessage current in Log.Messages)
			{
				float num2 = Text.CalcHeight(current.text, width);
				if (num2 > 30f)
				{
					num2 = 30f;
				}
				GUI.color = new Color(1f, 1f, 1f, 0.7f);
				Rect rect = new Rect(4f, num, 28f, num2);
				Widgets.Label(rect, current.repeats.ToStringCached());
				Rect rect2 = new Rect(28f, num, width, num2);
				if (EditWindow_Log.selectedMessage == current)
				{
					GUI.DrawTexture(rect2, EditWindow_Log.SelectedMessageTex);
				}
				else if (flag)
				{
					GUI.DrawTexture(rect2, EditWindow_Log.AltMessageTex);
				}
				if (Widgets.ButtonInvisible(rect2, false))
				{
					EditWindow_Log.ClearSelectedMessage();
					EditWindow_Log.selectedMessage = current;
				}
				GUI.color = current.Color;
				Widgets.Label(rect2, current.text);
				num += num2;
				flag = !flag;
			}
			if (Event.current.type == EventType.Layout)
			{
				this.listingViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.color = Color.white;
		}

		private void DoMessageDetails(Rect detailsRect, Rect outRect)
		{
			if (EditWindow_Log.selectedMessage == null)
			{
				return;
			}
			Rect rect = detailsRect;
			rect.height = 7f;
			Rect rect2 = detailsRect;
			rect2.yMin = rect.yMax;
			GUI.DrawTexture(rect, EditWindow_Log.StackTraceBorderTex);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
			{
				this.borderDragging = true;
				Event.current.Use();
			}
			if (this.borderDragging)
			{
				EditWindow_Log.detailsPaneHeight = outRect.height + Mathf.Round(3.5f) - Event.current.mousePosition.y;
			}
			if (Event.current.type == EventType.MouseUp)
			{
				this.borderDragging = false;
			}
			GUI.DrawTexture(rect2, EditWindow_Log.StackTraceAreaTex);
			Rect rect3 = new Rect(0f, 0f, rect2.width - 16f, 0f);
			string text = EditWindow_Log.selectedMessage.text + "\n" + EditWindow_Log.selectedMessage.StackTrace;
			float height = Text.CalcHeight(text, rect3.width);
			rect3.height = height;
			Widgets.BeginScrollView(rect2, ref EditWindow_Log.detailsScrollPosition, rect3);
			Widgets.Label(rect3, text);
			Widgets.EndScrollView();
		}
	}
}
