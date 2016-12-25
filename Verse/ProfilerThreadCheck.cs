using System;

namespace Verse
{
	public static class ProfilerThreadCheck
	{
		public static void BeginSample(string name)
		{
			if (UnityData.IsInMainThread)
			{
			}
		}

		public static void EndSample()
		{
			if (UnityData.IsInMainThread)
			{
			}
		}
	}
}
