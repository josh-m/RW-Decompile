using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenDate
	{
		public const int TicksPerDay = 60000;

		public const int HoursPerDay = 24;

		public const int DaysPerMonth = 5;

		public const int MonthsPerYear = 12;

		public const int GameStartHourOfDay = 6;

		public const float SecondsPerTickAsFractionOfDay = 1.44f;

		public const int TicksPerMonth = 300000;

		public const int TicksPerSeason = 900000;

		public const int TicksPerYear = 3600000;

		public const int DaysPerYear = 60;

		public const int DaysPerSeason = 15;

		public const int TicksPerHour = 2500;

		public const float TimeZoneWidth = 15f;

		public const int DefaultStartingYear = 5500;

		private static int TicksGame
		{
			get
			{
				return Find.TickManager.TicksGame;
			}
		}

		private static int TicksAbs
		{
			get
			{
				return Find.TickManager.TicksAbs;
			}
		}

		public static int DaysPassed
		{
			get
			{
				return GenDate.DaysPassedAt(GenDate.TicksGame);
			}
		}

		public static float DaysPassedFloat
		{
			get
			{
				return (float)GenDate.TicksGame / 60000f;
			}
		}

		public static int MonthsPassed
		{
			get
			{
				return GenDate.MonthsPassedAt(GenDate.TicksGame);
			}
		}

		public static float MonthsPassedFloat
		{
			get
			{
				return (float)GenDate.TicksGame / 300000f;
			}
		}

		public static int YearsPassed
		{
			get
			{
				return GenDate.YearsPassedAt(GenDate.TicksGame);
			}
		}

		public static float YearsPassedFloat
		{
			get
			{
				return (float)GenDate.TicksGame / 3600000f;
			}
		}

		public static int TickAbsToGame(int absTick)
		{
			return absTick - Find.TickManager.gameStartAbsTick;
		}

		public static int TickGameToAbs(int gameTick)
		{
			return gameTick + Find.TickManager.gameStartAbsTick;
		}

		public static int DaysPassedAt(int gameticks)
		{
			return gameticks / 60000;
		}

		public static int MonthsPassedAt(int gameticks)
		{
			return gameticks / 300000;
		}

		public static int YearsPassedAt(int gameTicks)
		{
			return gameTicks / 3600000;
		}

		private static long LocalTicksOffsetFromLongitude(float longitude)
		{
			return (long)GenDate.TimeZoneAt(longitude) * 2500L;
		}

		public static int HourOfDay(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			return (int)(num % 60000L / 2500L);
		}

		public static int DayOfMonth(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num / 60000L % 5L);
			if (num2 < 0)
			{
				num2 += 5;
			}
			return num2;
		}

		public static int DayOfYear(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num / 60000L) % 60;
			if (num2 < 0)
			{
				num2 += 60;
			}
			return num2;
		}

		public static Month Month(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num / 300000L % 12L);
			if (num2 < 0)
			{
				num2 += 12;
			}
			return (Month)num2;
		}

		public static Season Season(long absTicks, float longitude)
		{
			Month month = GenDate.Month(absTicks, longitude);
			return month.GetSeason();
		}

		public static int Year(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num / 3600000L);
			if (num < 0L)
			{
				num2--;
			}
			return 5500 + num2;
		}

		public static int DayOfSeason(long absTicks, float longitude)
		{
			int num = GenDate.DayOfYear(absTicks, longitude);
			return (num + 5) % 15;
		}

		public static float DayPercent(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num % 60000L);
			if (num2 == 0)
			{
				num2 = 1;
			}
			return (float)num2 / 60000f;
		}

		public static int HourInt(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num % 60000L);
			return num2 / 2500;
		}

		public static string DateFullStringAt(long absTicks, float longitude)
		{
			int num = GenDate.DayOfSeason(absTicks, longitude) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "FullDate".Translate(new object[]
			{
				text,
				GenDate.Season(absTicks, longitude).Label(),
				GenDate.Year(absTicks, longitude),
				num
			});
		}

		public static string DateReadoutStringAt(long absTicks, float longitude)
		{
			int num = GenDate.DayOfSeason(absTicks, longitude) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "DateReadout".Translate(new object[]
			{
				text,
				GenDate.Season(absTicks, longitude).Label(),
				GenDate.Year(absTicks, longitude),
				num
			});
		}

		public static string SeasonDateStringAt(long absTicks, float longitude)
		{
			int num = GenDate.DayOfSeason(absTicks, longitude) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "SeasonFullDate".Translate(new object[]
			{
				text,
				GenDate.Season(absTicks, longitude).Label(),
				num
			});
		}

		public static string SeasonDateStringAt(Month month)
		{
			return GenDate.SeasonDateStringAt((long)((int)month * 300000 + 1), 0f);
		}

		public static float TicksToDays(this int numTicks)
		{
			return (float)numTicks / 60000f;
		}

		public static string ToStringTicksToDays(this int numTicks)
		{
			return numTicks.TicksToDays().ToString("F1") + " " + "DaysLower".Translate();
		}

		public static string ToStringTicksToPeriod(this int numTicks, bool allowHours = true)
		{
			if (numTicks < 0)
			{
				return "0";
			}
			int num;
			int num2;
			int num3;
			float num4;
			numTicks.TicksToPeriod(out num, out num2, out num3, out num4);
			if (num > 0)
			{
				string text;
				if (num == 1)
				{
					text = "Period1Year".Translate();
				}
				else
				{
					text = "PeriodYears".Translate(new object[]
					{
						num
					});
				}
				if (num2 > 0)
				{
					text += ", ";
					if (num2 == 1)
					{
						text += "Period1Season".Translate();
					}
					else
					{
						text += "PeriodSeasons".Translate(new object[]
						{
							num2
						});
					}
				}
				return text;
			}
			if (num2 > 0)
			{
				string text2;
				if (num2 == 1)
				{
					text2 = "Period1Season".Translate();
				}
				else
				{
					text2 = "PeriodSeasons".Translate(new object[]
					{
						num2
					});
				}
				if (num3 > 0)
				{
					text2 += ", ";
					if (num3 == 1)
					{
						text2 += "Period1Day".Translate();
					}
					else
					{
						text2 += "PeriodDays".Translate(new object[]
						{
							num3
						});
					}
				}
				return text2;
			}
			if (num3 > 0)
			{
				string text3;
				if (num3 == 1)
				{
					text3 = "Period1Day".Translate();
				}
				else
				{
					text3 = "PeriodDays".Translate(new object[]
					{
						num3
					});
				}
				int num5 = (int)num4;
				if (allowHours && num5 > 0)
				{
					text3 += ", ";
					if (num5 == 1)
					{
						text3 += "Period1Hour".Translate();
					}
					else
					{
						text3 += "PeriodHours".Translate(new object[]
						{
							num5
						});
					}
				}
				return text3;
			}
			if (!allowHours)
			{
				return "LessThanADay".Translate();
			}
			if (Math.Round((double)num4, 2) == 1.0)
			{
				return "Period1Hour".Translate();
			}
			return "PeriodHours".Translate(new object[]
			{
				num4.ToStringDecimalIfSmall()
			});
		}

		public static string ToStringTicksToPeriodVagueMax(this int numTicks)
		{
			if (numTicks > 36000000)
			{
				return "OverADecade".Translate();
			}
			return numTicks.ToStringTicksToPeriod(false);
		}

		public static void TicksToPeriod(this int numTicks, out int years, out int seasons, out int days, out float hoursFloat)
		{
			((long)numTicks).TicksToPeriod(out years, out seasons, out days, out hoursFloat);
		}

		public static void TicksToPeriod(this long numTicks, out int years, out int seasons, out int days, out float hoursFloat)
		{
			years = (int)(numTicks / 3600000L);
			long num = numTicks - (long)years * 3600000L;
			seasons = (int)(num / 900000L);
			num -= (long)seasons * 900000L;
			days = (int)(num / 60000L);
			num -= (long)days * 60000L;
			hoursFloat = (float)num / 2500f;
		}

		public static string ToStringApproxAge(this float yearsFloat)
		{
			if (yearsFloat >= 1f)
			{
				return ((int)yearsFloat).ToStringCached();
			}
			int num = (int)(yearsFloat * 3600000f);
			num = Mathf.Min(num, 3599999);
			int num2;
			int num3;
			int num4;
			float num5;
			num.TicksToPeriod(out num2, out num3, out num4, out num5);
			if (num2 > 0)
			{
				if (num2 == 1)
				{
					return "Period1Year".Translate();
				}
				return "PeriodYears".Translate(new object[]
				{
					num2
				});
			}
			else if (num3 > 0)
			{
				if (num3 == 1)
				{
					return "Period1Season".Translate();
				}
				return "PeriodSeasons".Translate(new object[]
				{
					num3
				});
			}
			else if (num4 > 0)
			{
				if (num4 == 1)
				{
					return "Period1Day".Translate();
				}
				return "PeriodDays".Translate(new object[]
				{
					num4
				});
			}
			else
			{
				int num6 = (int)num5;
				if (num6 == 1)
				{
					return "Period1Hour".Translate();
				}
				return "PeriodHours".Translate(new object[]
				{
					num6
				});
			}
		}

		public static int TimeZoneAt(float longitude)
		{
			return Mathf.RoundToInt(longitude / 15f);
		}
	}
}
