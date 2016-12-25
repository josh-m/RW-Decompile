using System;

namespace Verse.AI
{
	public static class JobFailReason
	{
		private static string lastReason;

		public static string Reason
		{
			get
			{
				return JobFailReason.lastReason;
			}
		}

		public static bool HaveReason
		{
			get
			{
				return JobFailReason.lastReason != null;
			}
		}

		public static void Is(string reason)
		{
			JobFailReason.lastReason = reason;
		}

		public static void Clear()
		{
			JobFailReason.lastReason = null;
		}
	}
}
