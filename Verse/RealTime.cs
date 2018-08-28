using System;
using System.Threading;
using UnityEngine;

namespace Verse
{
	public static class RealTime
	{
		public static float deltaTime;

		public static float realDeltaTime;

		public static RealtimeMoteList moteList = new RealtimeMoteList();

		public static int frameCount;

		private static float lastRealTime = 0f;

		public static float LastRealTime
		{
			get
			{
				return RealTime.lastRealTime;
			}
		}

		public static void Update()
		{
			RealTime.frameCount = Time.frameCount;
			RealTime.deltaTime = Time.deltaTime;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			RealTime.realDeltaTime = realtimeSinceStartup - RealTime.lastRealTime;
			RealTime.lastRealTime = realtimeSinceStartup;
			if (Current.ProgramState == ProgramState.Playing)
			{
				RealTime.moteList.MoteListUpdate();
			}
			else
			{
				RealTime.moteList.Clear();
			}
			if (DebugSettings.lowFPS && Time.deltaTime < 100f)
			{
				Thread.Sleep((int)(100f - Time.deltaTime));
			}
		}
	}
}
