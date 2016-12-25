using System;
using System.Threading;
using UnityEngine;

namespace Verse
{
	public static class UnityData
	{
		public static bool isDebugBuild;

		public static bool isEditor;

		public static string dataPath;

		public static RuntimePlatform platform;

		public static string persistentDataPath;

		private static int mainThreadId;

		public static bool IsInMainThread
		{
			get
			{
				return UnityData.mainThreadId == Thread.CurrentThread.ManagedThreadId;
			}
		}

		public static void CopyUnityData()
		{
			UnityData.mainThreadId = Thread.CurrentThread.ManagedThreadId;
			UnityData.isDebugBuild = Debug.isDebugBuild;
			UnityData.isEditor = Application.isEditor;
			UnityData.dataPath = Application.dataPath;
			UnityData.platform = Application.platform;
			UnityData.persistentDataPath = Application.persistentDataPath;
		}
	}
}
