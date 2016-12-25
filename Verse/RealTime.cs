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
			RealTime.realDeltaTime = Time.realtimeSinceStartup - RealTime.lastRealTime;
			RealTime.lastRealTime = Time.realtimeSinceStartup;
			if (Current.ProgramState == ProgramState.MapPlaying)
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
