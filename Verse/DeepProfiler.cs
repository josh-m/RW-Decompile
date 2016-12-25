using System;
using System.Collections.Generic;
using System.Threading;

namespace Verse
{
	public static class DeepProfiler
	{
		private static Dictionary<int, ThreadLocalDeepProfiler> deepProfilers = new Dictionary<int, ThreadLocalDeepProfiler>();

		private static readonly object DeepProfilersLock = new object();

		public static ThreadLocalDeepProfiler Get()
		{
			object deepProfilersLock = DeepProfiler.DeepProfilersLock;
			ThreadLocalDeepProfiler result;
			lock (deepProfilersLock)
			{
				int managedThreadId = Thread.CurrentThread.ManagedThreadId;
				ThreadLocalDeepProfiler threadLocalDeepProfiler;
				if (!DeepProfiler.deepProfilers.TryGetValue(managedThreadId, out threadLocalDeepProfiler))
				{
					threadLocalDeepProfiler = new ThreadLocalDeepProfiler();
					DeepProfiler.deepProfilers.Add(managedThreadId, threadLocalDeepProfiler);
					result = threadLocalDeepProfiler;
				}
				else
				{
					result = threadLocalDeepProfiler;
				}
			}
			return result;
		}

		public static void Start(string label = null)
		{
			if (!Prefs.LogVerbose)
			{
				return;
			}
			DeepProfiler.Get().Start(label);
		}

		public static void End()
		{
			if (!Prefs.LogVerbose)
			{
				return;
			}
			DeepProfiler.Get().End();
		}
	}
}
