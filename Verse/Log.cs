using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class Log
	{
		private static LogMessageQueue messageQueue = new LogMessageQueue();

		private static HashSet<int> usedKeys = new HashSet<int>();

		public static bool openOnMessage = false;

		private static bool currentlyLoggingError;

		private static int messageCount;

		private const int StopLoggingAtMessageCount = 1000;

		public static IEnumerable<LogMessage> Messages
		{
			get
			{
				return Log.messageQueue.Messages;
			}
		}

		private static bool ReachedMaxMessagesLimit
		{
			get
			{
				return Log.messageCount >= 1000 && !UnityData.isDebugBuild;
			}
		}

		public static void ResetMessageCount()
		{
			bool reachedMaxMessagesLimit = Log.ReachedMaxMessagesLimit;
			Log.messageCount = 0;
			if (reachedMaxMessagesLimit)
			{
				Log.Message("Message logging is now once again on.", false);
			}
		}

		public static void Message(string text, bool ignoreStopLoggingLimit = false)
		{
			if (!ignoreStopLoggingLimit && Log.ReachedMaxMessagesLimit)
			{
				return;
			}
			Debug.Log(text);
			Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Message, text, StackTraceUtility.ExtractStackTrace()));
			Log.PostMessage();
		}

		public static void Warning(string text, bool ignoreStopLoggingLimit = false)
		{
			if (!ignoreStopLoggingLimit && Log.ReachedMaxMessagesLimit)
			{
				return;
			}
			Debug.LogWarning(text);
			Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Warning, text, StackTraceUtility.ExtractStackTrace()));
			Log.PostMessage();
		}

		public static void Error(string text, bool ignoreStopLoggingLimit = false)
		{
			if (!ignoreStopLoggingLimit && Log.ReachedMaxMessagesLimit)
			{
				return;
			}
			Debug.LogError(text);
			if (!Log.currentlyLoggingError)
			{
				Log.currentlyLoggingError = true;
				try
				{
					if (Prefs.PauseOnError && Current.ProgramState == ProgramState.Playing)
					{
						Find.TickManager.Pause();
					}
					Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Error, text, StackTraceUtility.ExtractStackTrace()));
					Log.PostMessage();
					if (!PlayDataLoader.Loaded || Prefs.DevMode)
					{
						Log.TryOpenLogWindow();
					}
				}
				catch (Exception arg)
				{
					Debug.LogError("An error occurred while logging an error: " + arg);
				}
				finally
				{
					Log.currentlyLoggingError = false;
				}
			}
		}

		public static void ErrorOnce(string text, int key, bool ignoreStopLoggingLimit = false)
		{
			if (!ignoreStopLoggingLimit && Log.ReachedMaxMessagesLimit)
			{
				return;
			}
			if (Log.usedKeys.Contains(key))
			{
				return;
			}
			Log.usedKeys.Add(key);
			Log.Error(text, ignoreStopLoggingLimit);
		}

		public static void Clear()
		{
			EditWindow_Log.ClearSelectedMessage();
			Log.messageQueue.Clear();
			Log.ResetMessageCount();
		}

		public static void TryOpenLogWindow()
		{
			if (StaticConstructorOnStartupUtility.coreStaticAssetsLoaded || UnityData.IsInMainThread)
			{
				EditWindow_Log.TryAutoOpen();
			}
		}

		private static void PostMessage()
		{
			if (Log.openOnMessage)
			{
				Log.TryOpenLogWindow();
				EditWindow_Log.SelectLastMessage(true);
			}
			Log.messageCount++;
			if (Log.messageCount == 1000 && Log.ReachedMaxMessagesLimit)
			{
				Log.Warning("Reached max messages limit. Stopping logging to avoid spam.", true);
			}
		}
	}
}
