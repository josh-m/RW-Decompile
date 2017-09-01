using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenDate
	{
		public const int TicksPerDay = 60000;

		public const int HoursPerDay = 24;

		public const int DaysPerTwelfth = 5;

		public const int TwelfthsPerYear = 12;

		public const int GameStartHourOfDay = 6;

		public const float SecondsPerTickAsFractionOfDay = 1.44f;

		public const int TicksPerTwelfth = 300000;

		public const int TicksPerSeason = 900000;

		public const int TicksPerQuadrum = 900000;

		public const int TicksPerYear = 3600000;

		public const int DaysPerYear = 60;

		public const int DaysPerSeason = 15;

		public const int DaysPerQuadrum = 15;

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

		public static int TwelfthsPassed
		{
			get
			{
				return GenDate.TwelfthsPassedAt(GenDate.TicksGame);
			}
		}

		public static float TwelfthsPassedFloat
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

		public static int TwelfthsPassedAt(int gameticks)
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

		public static int DayOfTwelfth(long absTicks, float longitude)
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

		public static Twelfth Twelfth(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num / 300000L % 12L);
			if (num2 < 0)
			{
				num2 += 12;
			}
			return (Twelfth)num2;
		}

		public static Season Season(long absTicks, Vector2 longLat)
		{
			return GenDate.Season(absTicks, longLat.y, longLat.x);
		}

		public static Season Season(long absTicks, float latitude, float longitude)
		{
			Twelfth twelfth = GenDate.Twelfth(absTicks, longitude);
			return twelfth.GetSeason(latitude);
		}

		public static Quadrum Quadrum(long absTicks, float longitude)
		{
			Twelfth twelfth = GenDate.Twelfth(absTicks, longitude);
			return twelfth.GetQuadrum();
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
			return (num - (int)(SeasonUtility.FirstSeason.GetFirstTwelfth(0f) * RimWorld.Twelfth.Sixth)) % 15;
		}

		public static int DayOfQuadrum(long absTicks, float longitude)
		{
			int num = GenDate.DayOfYear(absTicks, longitude);
			return (num - (int)(QuadrumUtility.FirstQuadrum.GetFirstTwelfth() * RimWorld.Twelfth.Sixth)) % 15;
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

		public static int HourInteger(long absTicks, float longitude)
		{
			long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
			int num2 = (int)(num % 60000L);
			return num2 / 2500;
		}

		public static float HourFloat(long absTicks, float longitude)
		{
			return GenDate.DayPercent(absTicks, longitude) * 24f;
		}

		public static string DateFullStringAt(long absTicks, Vector2 location)
		{
			int num = GenDate.DayOfSeason(absTicks, location.x) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "FullDate".Translate(new object[]
			{
				text,
				GenDate.Quadrum(absTicks, location.x).Label(),
				GenDate.Year(absTicks, location.x),
				num
			});
		}

		public static string DateReadoutStringAt(long absTicks, Vector2 location)
		{
			int num = GenDate.DayOfSeason(absTicks, location.x) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "DateReadout".Translate(new object[]
			{
				text,
				GenDate.Quadrum(absTicks, location.x).Label(),
				GenDate.Year(absTicks, location.x),
				num
			});
		}

		public static string SeasonDateStringAt(long absTicks, Vector2 longLat)
		{
			int num = GenDate.DayOfSeason(absTicks, longLat.x) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "SeasonFullDate".Translate(new object[]
			{
				text,
				GenDate.Season(absTicks, longLat).Label(),
				num
			});
		}

		public static string SeasonDateStringAt(Twelfth twelfth, Vector2 longLat)
		{
			return GenDate.SeasonDateStringAt((long)((int)twelfth * 300000 + 1), longLat);
		}

		public static string QuadrumDateStringAt(long absTicks, float longitude)
		{
			int num = GenDate.DayOfQuadrum(absTicks, longitude) + 1;
			string text = Find.ActiveLanguageWorker.OrdinalNumber(num);
			return "SeasonFullDate".Translate(new object[]
			{
				text,
				GenDate.Quadrum(absTicks, longitude).Label(),
				num
			});
		}

		public static string QuadrumDateStringAt(Quadrum quadrum)
		{
			return GenDate.QuadrumDateStringAt((long)((int)quadrum * 900000 + 1), 0f);
		}

		public static string QuadrumDateStringAt(Twelfth twelfth)
		{
			return GenDate.QuadrumDateStringAt((long)((int)twelfth * 300000 + 1), 0f);
		}

		public static float TicksToDays(this int numTicks)
		{
			return (float)numTicks / 60000f;
		}

		public static string ToStringTicksToDays(this int numTicks, string format = "F1")
		{
			return numTicks.TicksToDays().ToString(format) + " " + "DaysLower".Translate();
		}

		public static string ToStringTicksToPeriod(this int numTicks, bool allowHours = true, bool hoursMax1DecimalPlace = false, bool allowQuadrums = true)
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
			if (!allowQuadrums)
			{
				num3 += 15 * num2;
				num2 = 0;
			}
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
						text += "Period1Quadrum".Translate();
					}
					else
					{
						text += "PeriodQuadrums".Translate(new object[]
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
					text2 = "Period1Quadrum".Translate();
				}
				else
				{
					text2 = "PeriodQuadrums".Translate(new object[]
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
			if (hoursMax1DecimalPlace)
			{
				if (num4 > 1f)
				{
					int num6 = Mathf.RoundToInt(num4);
					if (num6 == 1)
					{
						return "Period1Hour".Translate();
					}
					return "PeriodHours".Translate(new object[]
					{
						num6
					});
				}
				else
				{
					if (Math.Round((double)num4, 1) == 1.0)
					{
						return "Period1Hour".Translate();
					}
					return "PeriodHours".Translate(new object[]
					{
						num4.ToString("0.#")
					});
				}
			}
			else
			{
				if (Math.Round((double)num4, 2) == 1.0)
				{
					return "Period1Hour".Translate();
				}
				return "PeriodHours".Translate(new object[]
				{
					num4.ToStringDecimalIfSmall()
				});
			}
		}

		public static string ToStringTicksToPeriodVagueMax(this int numTicks)
		{
			if (numTicks > 36000000)
			{
				return "OverADecade".Translate();
			}
			return numTicks.ToStringTicksToPeriod(false, false, true);
		}

		public static void TicksToPeriod(this int numTicks, out int years, out int quadrums, out int days, out float hoursFloat)
		{
			((long)numTicks).TicksToPeriod(out years, out quadrums, out days, out hoursFloat);
		}

		public static void TicksToPeriod(this long numTicks, out int years, out int quadrums, out int days, out float hoursFloat)
		{
			years = (int)(numTicks / 3600000L);
			long num = numTicks - (long)years * 3600000L;
			quadrums = (int)(num / 900000L);
			num -= (long)quadrums * 900000L;
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
					return "Period1Quadrum".Translate();
				}
				return "PeriodQuadrums".Translate(new object[]
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
			return Mathf.RoundToInt(GenDate.TimeZoneFloatAt(longitude));
		}

		public static float TimeZoneFloatAt(float longitude)
		{
			return longitude / 15f;
		}
	}
}
