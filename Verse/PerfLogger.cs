using System;
using System.Diagnostics;
using System.Text;

namespace Verse
{
	public static class PerfLogger
	{
		public static StringBuilder currentLog = new StringBuilder();

		private static long start;

		private static long current;

		private static int indent;

		public static void Reset()
		{
			PerfLogger.currentLog = null;
			PerfLogger.start = Stopwatch.GetTimestamp();
			PerfLogger.current = PerfLogger.start;
		}

		public static void Flush()
		{
			Log.Message((PerfLogger.currentLog == null) ? string.Empty : PerfLogger.currentLog.ToString());
			PerfLogger.Reset();
		}

		public static void Record(string label)
		{
			long timestamp = Stopwatch.GetTimestamp();
			if (PerfLogger.currentLog == null)
			{
				PerfLogger.currentLog = new StringBuilder();
			}
			PerfLogger.currentLog.AppendLine(string.Format("{0}: {3}{1} ({2})", new object[]
			{
				(timestamp - PerfLogger.start) * 1000L / Stopwatch.Frequency,
				label,
				(timestamp - PerfLogger.current) * 1000L / Stopwatch.Frequency,
				new string(' ', PerfLogger.indent * 2)
			}));
			PerfLogger.current = timestamp;
		}

		public static void Indent()
		{
			PerfLogger.indent++;
		}

		public static void Outdent()
		{
			PerfLogger.indent--;
		}

		public static float Duration()
		{
			return (float)(Stopwatch.GetTimestamp() - PerfLogger.start) / (float)Stopwatch.Frequency;
		}
	}
}
