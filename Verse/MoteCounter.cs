using System;

namespace Verse
{
	public static class MoteCounter
	{
		private const int SaturatedCount = 250;

		private static int moteCount;

		public static int MoteCount
		{
			get
			{
				return MoteCounter.moteCount;
			}
		}

		public static float Saturation
		{
			get
			{
				return (float)MoteCounter.moteCount / 250f;
			}
		}

		public static bool Saturated
		{
			get
			{
				return MoteCounter.Saturation > 1f;
			}
		}

		public static bool SaturatedLowPriority
		{
			get
			{
				return MoteCounter.Saturation > 0.8f;
			}
		}

		public static void Reinit()
		{
			MoteCounter.moteCount = 0;
		}

		public static void Notify_MoteSpawned()
		{
			MoteCounter.moteCount++;
		}

		public static void Notify_MoteDespawned()
		{
			MoteCounter.moteCount--;
		}
	}
}
