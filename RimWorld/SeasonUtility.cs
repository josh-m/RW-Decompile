using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class SeasonUtility
	{
		public static Month GetFirstMonth(this Season season)
		{
			switch (season)
			{
			case Season.Spring:
				return Month.Mar;
			case Season.Summer:
				return Month.Jun;
			case Season.Fall:
				return Month.Sept;
			case Season.Winter:
				return Month.Dec;
			default:
				return Month.Undefined;
			}
		}

		public static string Label(this Season season)
		{
			switch (season)
			{
			case Season.Spring:
				return "SeasonSpring".Translate();
			case Season.Summer:
				return "SeasonSummer".Translate();
			case Season.Fall:
				return "SeasonFall".Translate();
			case Season.Winter:
				return "SeasonWinter".Translate();
			default:
				return "Unknown season";
			}
		}

		public static string LabelCap(this Season season)
		{
			return season.Label().CapitalizeFirst();
		}

		public static string SeasonsRangeLabel(List<Month> months)
		{
			if (months.Count == 0)
			{
				return string.Empty;
			}
			if (months.Count == 12)
			{
				return "WholeYear".Translate();
			}
			string text = string.Empty;
			for (int i = 0; i < 12; i++)
			{
				Month month = (Month)i;
				if (months.Contains(month))
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += SeasonUtility.SeasonsContinuousRangeLabel(months, month);
				}
			}
			return text;
		}

		private static string SeasonsContinuousRangeLabel(List<Month> months, Month rootMonth)
		{
			Month leftMostMonth = SeasonUtility.GetLeftMostMonth(months, rootMonth);
			Month rightMostMonth = SeasonUtility.GetRightMostMonth(months, rootMonth);
			for (Month month = leftMostMonth; month != rightMostMonth; month = SeasonUtility.MonthAfter(month))
			{
				if (!months.Contains(month))
				{
					Log.Error(string.Concat(new object[]
					{
						"Months doesn't contain ",
						month,
						" (",
						leftMostMonth,
						"..",
						rightMostMonth,
						")"
					}));
					break;
				}
				months.Remove(month);
			}
			months.Remove(rightMostMonth);
			return GenDate.SeasonDateStringAt(leftMostMonth) + " - " + GenDate.SeasonDateStringAt(rightMostMonth);
		}

		private static Month GetLeftMostMonth(List<Month> months, Month rootMonth)
		{
			if (months.Count >= 12)
			{
				return Month.Undefined;
			}
			Month result;
			do
			{
				result = rootMonth;
				rootMonth = SeasonUtility.MonthBefore(rootMonth);
			}
			while (months.Contains(rootMonth));
			return result;
		}

		private static Month GetRightMostMonth(List<Month> months, Month rootMonth)
		{
			if (months.Count >= 12)
			{
				return Month.Undefined;
			}
			Month m;
			do
			{
				m = rootMonth;
				rootMonth = SeasonUtility.MonthAfter(rootMonth);
			}
			while (months.Contains(rootMonth));
			return SeasonUtility.MonthAfter(m);
		}

		private static Month MonthBefore(Month m)
		{
			if (m == Month.Jan)
			{
				return Month.Dec;
			}
			return (Month)(m - Month.Feb);
		}

		private static Month MonthAfter(Month m)
		{
			if (m == Month.Dec)
			{
				return Month.Jan;
			}
			return m + 1;
		}
	}
}
