using System;

namespace Verse
{
	public static class UnityDataInitializer
	{
		public static bool initializing;

		public static void CopyUnityData()
		{
			UnityDataInitializer.initializing = true;
			try
			{
				UnityData.CopyUnityData();
			}
			finally
			{
				UnityDataInitializer.initializing = false;
			}
		}
	}
}
