using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verse
{
	public static class LongEventHandler
	{
		private class QueuedLongEvent
		{
			public Action eventAction;

			public string levelToLoad;

			public string eventTextKey = string.Empty;

			public string eventText = string.Empty;

			public bool doAsynchronously;

			public Action<Exception> exceptionHandler;

			public bool alreadyDisplayed;
		}

		private static Queue<LongEventHandler.QueuedLongEvent> eventQueue = new Queue<LongEventHandler.QueuedLongEvent>();

		private static LongEventHandler.QueuedLongEvent currentEvent = null;

		private static Thread eventThread = null;

		private static AsyncOperation levelLoadOp = null;

		private static List<Action> toExecuteWhenFinished = new List<Action>();

		private static bool executingToExecuteWhenFinished = false;

		private static readonly object CurrentEventTextLock = new object();

		private static readonly Vector2 GUIRectSize = new Vector2(240f, 75f);

		public static bool ShouldWaitForEvent
		{
			get
			{
				if (!LongEventHandler.AnyEventNowOrWaiting)
				{
					return false;
				}
				if (LongEventHandler.currentEvent != null && !LongEventHandler.ShouldWaitUntilDisplayed(LongEventHandler.currentEvent))
				{
					return true;
				}
				if (LongEventHandler.currentEvent == null && LongEventHandler.eventQueue.Any<LongEventHandler.QueuedLongEvent>())
				{
					LongEventHandler.QueuedLongEvent queuedLongEvent = LongEventHandler.eventQueue.Peek();
					if (queuedLongEvent.doAsynchronously)
					{
						return true;
					}
					if (!LongEventHandler.ShouldWaitUntilDisplayed(queuedLongEvent))
					{
						return true;
					}
				}
				return Find.UIRoot == null || Find.WindowStack == null;
			}
		}

		public static bool AnyEventNowOrWaiting
		{
			get
			{
				return LongEventHandler.currentEvent != null || LongEventHandler.eventQueue.Count > 0;
			}
		}

		public static void QueueLongEvent(Action action, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler)
		{
			LongEventHandler.QueuedLongEvent queuedLongEvent = new LongEventHandler.QueuedLongEvent();
			queuedLongEvent.eventAction = action;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			LongEventHandler.eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(Action preLoadLevelAction, string levelToLoad, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler)
		{
			LongEventHandler.QueuedLongEvent queuedLongEvent = new LongEventHandler.QueuedLongEvent();
			queuedLongEvent.eventAction = preLoadLevelAction;
			queuedLongEvent.levelToLoad = levelToLoad;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			LongEventHandler.eventQueue.Enqueue(queuedLongEvent);
		}

		public static void ClearQueuedEvents()
		{
			LongEventHandler.eventQueue.Clear();
		}

		public static void LongEventsOnGUI()
		{
			if (LongEventHandler.currentEvent == null)
			{
				return;
			}
			float num = LongEventHandler.GUIRectSize.x;
			object currentEventTextLock = LongEventHandler.CurrentEventTextLock;
			lock (currentEventTextLock)
			{
				Text.Font = GameFont.Small;
				num = Mathf.Max(num, Text.CalcSize(LongEventHandler.currentEvent.eventText + "...").x + 40f);
			}
			Rect rect = new Rect(((float)UI.screenWidth - num) / 2f, ((float)UI.screenHeight - LongEventHandler.GUIRectSize.y) / 2f, num, LongEventHandler.GUIRectSize.y);
			rect = rect.Rounded();
			if (LongEventHandler.currentEvent.doAsynchronously || Find.UIRoot == null || Find.WindowStack == null)
			{
				if (UIMenuBackgroundManager.background == null)
				{
					UIMenuBackgroundManager.background = new UI_BackgroundMain();
				}
				UIMenuBackgroundManager.background.BackgroundOnGUI();
				Widgets.DrawShadowAround(rect);
				Widgets.DrawWindowBackground(rect);
				LongEventHandler.DrawLongEventWindowContents(rect);
			}
			else
			{
				Find.WindowStack.ImmediateWindow(62893994, rect, WindowLayer.Super, delegate
				{
					LongEventHandler.DrawLongEventWindowContents(rect.AtZero());
				}, true, false, 1f);
			}
		}

		public static void LongEventsUpdate(out bool sceneChanged)
		{
			sceneChanged = false;
			if (LongEventHandler.currentEvent != null)
			{
				if (LongEventHandler.currentEvent.doAsynchronously)
				{
					LongEventHandler.UpdateCurrentAsynchronousEvent();
				}
				else
				{
					LongEventHandler.UpdateCurrentSynchronousEvent(out sceneChanged);
				}
			}
			if (LongEventHandler.currentEvent == null && LongEventHandler.eventQueue.Count > 0)
			{
				LongEventHandler.currentEvent = LongEventHandler.eventQueue.Dequeue();
				if (LongEventHandler.currentEvent.eventTextKey == null)
				{
					LongEventHandler.currentEvent.eventText = string.Empty;
				}
				else
				{
					LongEventHandler.currentEvent.eventText = LongEventHandler.currentEvent.eventTextKey.Translate();
				}
			}
		}

		public static void ExecuteWhenFinished(Action action)
		{
			LongEventHandler.toExecuteWhenFinished.Add(action);
			if ((LongEventHandler.currentEvent == null || LongEventHandler.ShouldWaitUntilDisplayed(LongEventHandler.currentEvent)) && !LongEventHandler.executingToExecuteWhenFinished)
			{
				LongEventHandler.ExecuteToExecuteWhenFinished();
			}
		}

		public static void SetCurrentEventText(string newText)
		{
			object currentEventTextLock = LongEventHandler.CurrentEventTextLock;
			lock (currentEventTextLock)
			{
				if (LongEventHandler.currentEvent != null)
				{
					LongEventHandler.currentEvent.eventText = newText;
				}
			}
		}

		private static void UpdateCurrentAsynchronousEvent()
		{
			if (LongEventHandler.eventThread == null)
			{
				LongEventHandler.eventThread = new Thread(delegate
				{
					LongEventHandler.RunEventFromAnotherThread(LongEventHandler.currentEvent.eventAction);
				});
				LongEventHandler.eventThread.Start();
			}
			else if (!LongEventHandler.eventThread.IsAlive)
			{
				bool flag = false;
				if (!LongEventHandler.currentEvent.levelToLoad.NullOrEmpty())
				{
					if (LongEventHandler.levelLoadOp == null)
					{
						LongEventHandler.levelLoadOp = SceneManager.LoadSceneAsync(LongEventHandler.currentEvent.levelToLoad);
					}
					else if (LongEventHandler.levelLoadOp.isDone)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					LongEventHandler.currentEvent = null;
					LongEventHandler.eventThread = null;
					LongEventHandler.levelLoadOp = null;
					LongEventHandler.ExecuteToExecuteWhenFinished();
				}
			}
		}

		private static void UpdateCurrentSynchronousEvent(out bool sceneChanged)
		{
			sceneChanged = false;
			if (LongEventHandler.ShouldWaitUntilDisplayed(LongEventHandler.currentEvent))
			{
				return;
			}
			try
			{
				if (LongEventHandler.currentEvent.eventAction != null)
				{
					LongEventHandler.currentEvent.eventAction();
				}
				if (!LongEventHandler.currentEvent.levelToLoad.NullOrEmpty())
				{
					SceneManager.LoadScene(LongEventHandler.currentEvent.levelToLoad);
					sceneChanged = true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception from long event: " + ex);
				if (LongEventHandler.currentEvent.exceptionHandler != null)
				{
					LongEventHandler.currentEvent.exceptionHandler(ex);
				}
			}
			finally
			{
				LongEventHandler.currentEvent = null;
				LongEventHandler.eventThread = null;
				LongEventHandler.levelLoadOp = null;
				LongEventHandler.ExecuteToExecuteWhenFinished();
			}
		}

		private static void RunEventFromAnotherThread(Action action)
		{
			try
			{
				if (action != null)
				{
					action();
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception from asynchronous event: " + ex);
				try
				{
					if (LongEventHandler.currentEvent.exceptionHandler != null)
					{
						LongEventHandler.currentEvent.exceptionHandler(ex);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Exception was thrown while trying to handle exception. Exception: " + arg);
				}
			}
		}

		private static void ExecuteToExecuteWhenFinished()
		{
			if (LongEventHandler.executingToExecuteWhenFinished)
			{
				Log.Warning("Already executing.");
				return;
			}
			LongEventHandler.executingToExecuteWhenFinished = true;
			for (int i = 0; i < LongEventHandler.toExecuteWhenFinished.Count; i++)
			{
				try
				{
					LongEventHandler.toExecuteWhenFinished[i]();
				}
				catch (Exception arg)
				{
					Log.Error("Could not execute post-long-event action. Exception: " + arg);
				}
			}
			LongEventHandler.toExecuteWhenFinished.Clear();
			LongEventHandler.executingToExecuteWhenFinished = false;
		}

		private static void DrawLongEventWindowContents(Rect rect)
		{
			if (LongEventHandler.currentEvent == null)
			{
				return;
			}
			if (Event.current.type == EventType.Repaint)
			{
				LongEventHandler.currentEvent.alreadyDisplayed = true;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			float num = 0f;
			if (LongEventHandler.levelLoadOp != null)
			{
				float f = 1f;
				if (!LongEventHandler.levelLoadOp.isDone)
				{
					f = LongEventHandler.levelLoadOp.progress;
				}
				string text = "LoadingAssets".Translate() + " " + f.ToStringPercent();
				num = Text.CalcSize(text).x;
				Widgets.Label(rect, text);
			}
			else
			{
				object currentEventTextLock = LongEventHandler.CurrentEventTextLock;
				lock (currentEventTextLock)
				{
					num = Text.CalcSize(LongEventHandler.currentEvent.eventText).x;
					Widgets.Label(rect, LongEventHandler.currentEvent.eventText);
				}
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			rect.xMin = rect.center.x + num / 2f;
			Widgets.Label(rect, LongEventHandler.currentEvent.doAsynchronously ? GenText.MarchingEllipsis(0f) : "...");
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private static bool ShouldWaitUntilDisplayed(LongEventHandler.QueuedLongEvent ev)
		{
			return !ev.doAsynchronously && !ev.alreadyDisplayed && !ev.eventText.NullOrEmpty();
		}
	}
}
