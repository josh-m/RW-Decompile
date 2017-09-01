using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanRestUtility
	{
		public const float WakeUpHour = 6f;

		public const float RestStartHour = 22f;

		public static bool RestingNowAt(int tile)
		{
			return CaravanRestUtility.WouldBeRestingAt(tile, (long)GenTicks.TicksAbs);
		}

		public static bool WouldBeRestingAt(int tile, long ticksAbs)
		{
			float num = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
			return num < 6f || num > 22f;
		}

		public static int LeftRestTicksAt(int tile)
		{
			return CaravanRestUtility.LeftRestTicksAt(tile, (long)GenTicks.TicksAbs);
		}

		public static int LeftRestTicksAt(int tile, long ticksAbs)
		{
			if (!CaravanRestUtility.WouldBeRestingAt(tile, ticksAbs))
			{
				return 0;
			}
			float num = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
			if (num < 6f)
			{
				return Mathf.CeilToInt((6f - num) * 2500f);
			}
			return Mathf.CeilToInt((24f - num + 6f) * 2500f);
		}

		public static int LeftNonRestTicksAt(int tile)
		{
			return CaravanRestUtility.LeftNonRestTicksAt(tile, (long)GenTicks.TicksAbs);
		}

		public static int LeftNonRestTicksAt(int tile, long ticksAbs)
		{
			if (CaravanRestUtility.WouldBeRestingAt(tile, ticksAbs))
			{
				return 0;
			}
			float num = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
			return Mathf.CeilToInt((22f - num) * 2500f);
		}

		public static float DayPercentNotRestingAt(int tile)
		{
			return Mathf.InverseLerp(6f, 22f, GenLocalDate.HourFloat(tile));
		}
	}
}
