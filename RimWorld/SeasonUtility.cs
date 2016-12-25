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
					while (i < 12 && months.Contains((Month)i))
					{
						i++;
					}
				}
			}
			return text;
		}

		private static string SeasonsContinuousRangeLabel(List<Month> months, Month month)
		{
			Month leftMostMonth = SeasonUtility.GetLeftMostMonth(months, month);
			Month month2 = SeasonUtility.GetRightMostMonth(months, month);
			if (month2 == Month.Dec)
			{
				month2 = Month.Jan;
			}
			else
			{
				month2 += 1;
			}
			return GenDate.SeasonDateStringAt(leftMostMonth) + " - " + GenDate.SeasonDateStringAt(month2);
		}

		private static Month GetLeftMostMonth(List<Month> months, Month month)
		{
			if (months.Count >= 12)
			{
				return Month.Undefined;
			}
			Month result;
			do
			{
				result = month;
				if (month == Month.Jan)
				{
					month = Month.Dec;
				}
				else
				{
					month -= 1;
				}
			}
			while (months.Contains(month));
			return result;
		}

		private static Month GetRightMostMonth(List<Month> months, Month month)
		{
			if (months.Count >= 12)
			{
				return Month.Undefined;
			}
			Month result;
			do
			{
				result = month;
				if (month == Month.Dec)
				{
					month = Month.Jan;
				}
				else
				{
					month += 1;
				}
			}
			while (months.Contains(month));
			return result;
		}
	}
}
