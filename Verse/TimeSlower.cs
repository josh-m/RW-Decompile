using System;

namespace Verse
{
	public class TimeSlower
	{
		private const int ForceTicksStandard = 790;

		private const int ForceTicksShort = 250;

		private int forceNormalSpeedUntil;

		public bool ForcedNormalSpeed
		{
			get
			{
				return Find.TickManager.TicksGame < this.forceNormalSpeedUntil;
			}
		}

		public void SignalForceNormalSpeed()
		{
			this.forceNormalSpeedUntil = Find.TickManager.TicksGame + 790;
		}

		public void SignalForceNormalSpeedShort()
		{
			this.forceNormalSpeedUntil = Find.TickManager.TicksGame + 250;
		}
	}
}
