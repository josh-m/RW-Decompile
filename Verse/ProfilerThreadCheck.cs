using System;
using System.Diagnostics;

namespace Verse
{
	public static class ProfilerThreadCheck
	{
		[Conditional("UNITY_EDITOR")]
		public static void BeginSample(string name)
		{
			if (UnityData.IsInMainThread)
			{
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void EndSample()
		{
			if (UnityData.IsInMainThread)
			{
			}
		}
	}
}
