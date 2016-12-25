using System;

namespace Verse
{
	public class MoteCounter
	{
		private const int SaturatedCount = 250;

		private int moteCount;

		public int MoteCount
		{
			get
			{
				return this.moteCount;
			}
		}

		public float Saturation
		{
			get
			{
				return (float)this.moteCount / 250f;
			}
		}

		public bool Saturated
		{
			get
			{
				return this.Saturation > 1f;
			}
		}

		public bool SaturatedLowPriority
		{
			get
			{
				return this.Saturation > 0.8f;
			}
		}

		public void Notify_MoteSpawned()
		{
			this.moteCount++;
		}

		public void Notify_MoteDespawned()
		{
			this.moteCount--;
		}
	}
}
