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
			float num;
			if (!MessagesRepeatAvoider.lastShowTimes.TryGetValue(tag, out num))
			{
				num = -99999f;
			}
			bool flag = RealTime.LastRealTime > num + minSecondsSinceLastShow;
			if (flag)
			{
				MessagesRepeatAvoider.lastShowTimes[tag] = RealTime.LastRealTime;
			}
			return flag;
		}
	}
}
