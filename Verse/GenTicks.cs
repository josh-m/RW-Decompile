using RimWorld;
using System;
using UnityEngine;

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
				if (Find.GameInitData == null || Current.ProgramState == ProgramState.Playing)
				{
					return Find.TickManager.TicksAbs;
				}
				if (Current.Game != null && Find.GameInitData != null)
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
				GameInitData gameInitData = Find.GameInitData;
				float longitude;
				if (gameInitData.startingTile >= 0)
				{
					longitude = Find.WorldGrid.LongLatOf(gameInitData.startingTile).x;
				}
				else
				{
					longitude = 0f;
				}
				Month month;
				if (gameInitData.startingMonth != Month.Undefined)
				{
					month = gameInitData.startingMonth;
				}
				else
				{
					month = Month.Jan;
				}
				int num = (24 - GenDate.TimeZoneAt(longitude)) % 24;
				return 300000 * (int)month + 2500 * (6 + num);
			}
		}

		public static float TicksToSeconds(this int numTicks)
		{
			return (float)numTicks / 60f;
		}

		public static int SecondsToTicks(this float numSeconds)
		{
			return Mathf.RoundToInt(60f * numSeconds);
		}

		public static string TickstoSecondsString(this int numTicks)
		{
			return numTicks.TicksToSeconds().ToString("F1") + " " + "SecondsLower".Translate();
		}

		public static string SecondsToTicksString(this float numSeconds)
		{
			return numSeconds.SecondsToTicks().ToString();
		}
	}
}
