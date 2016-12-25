using System;
using Verse;

namespace RimWorld
{
	public static class DateUtility
	{
		private static Season lastSeason;

		public static void DatesTick()
		{
			Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
			if (anyPlayerHomeMap == null)
			{
				return;
			}
			Season season = GenLocalDate.Season(anyPlayerHomeMap);
			if (season != DateUtility.lastSeason)
			{
				if (DateUtility.lastSeason != Season.Undefined && anyPlayerHomeMap.mapTemperature.LocalSeasonsAreMeaningful())
				{
					if (GenDate.YearsPassed == 0 && season == Season.Summer && GenTemperature.AverageTemperatureAtTileForMonth(anyPlayerHomeMap.Tile, Month.Jan) < 8f)
					{
						Find.LetterStack.ReceiveLetter(new Letter("LetterLabelFirstSummerWarning".Translate(), "FirstSummerWarning".Translate(), LetterType.Good), null);
					}
					else if (GenDate.DaysPassed > 5)
					{
						Messages.Message("MessageSeasonBegun".Translate(new object[]
						{
							season.Label()
						}).CapitalizeFirst(), MessageSound.Standard);
					}
				}
				DateUtility.lastSeason = season;
			}
		}
	}
}
