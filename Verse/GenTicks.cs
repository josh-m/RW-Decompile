using System;

namespace Verse
{
	public static class GenTicks
	{
		public const int TicksPerRealSecond = 60;

		public const int TickRareInterval = 250;

		public const int TickLongInterval = 2000;

		public static int TicksAbs
		{
			get
			{
				if (Current.ProgramState == ProgramState.MapPlaying)
				{
					return Find.TickManager.TicksAbs;
				}
				if (Current.Game != null)
				{
					return GenTicks.ConfiguredTicksAbsAtGameStart;
				}
				return 0;
			}
		}

		public static int ConfiguredTicksAbsAtGameStart
		{
			get
			{
				return 300000 * (int)Find.GameInitData.startingMonth + 15000;
			}
		}
	}
}
