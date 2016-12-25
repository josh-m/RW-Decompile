using System;
using UnityEngine;

namespace Verse
{
	public static class RealTime
	{
		public static float realDeltaTime = 0f;

		public static RealtimeMoteList moteList = new RealtimeMoteList();

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
		}
	}
}
