using System;
using System.Collections.Generic;

namespace Verse
{
	public static class MessagesRepeatAvoider
	{
		private static Dictionary<string, float> lastShowTimes = new Dictionary<string, float>();

		public static void Reset()
		{
			MessagesRepeatAvoider.lastShowTimes.Clear();
		}

		public static bool MessageShowAllowed(string tag, float minSecondsSinceLastShow)
		{
			float num = -999f;
			MessagesRepeatAvoider.lastShowTimes.TryGetValue(tag, out num);
			bool result = RealTime.LastRealTime > num + minSecondsSinceLastShow;
			MessagesRepeatAvoider.lastShowTimes[tag] = RealTime.LastRealTime;
			return result;
		}
	}
}
