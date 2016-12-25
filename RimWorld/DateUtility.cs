using System;
using Verse;

namespace RimWorld
{
	public static class DateUtility
	{
		private static Season lastSeason;

		public static void DatesTick()
		{
			Season currentSeason = GenDate.CurrentSeason;
			if (currentSeason != DateUtility.lastSeason)
			{
				if (DateUtility.lastSeason != Season.Undefined && GenTemperature.LocalSeasonsAreMeaningful())
				{
					if (GenDate.YearsPassed == 0 && currentSeason == Season.Summer && GenTemperature.AverageTemperatureAtWorldCoordsForMonth(Find.Map.WorldCoords, Month.Jan) < 8f)
					{
						Find.LetterStack.ReceiveLetter(new Letter("LetterLabelFirstSummerWarning".Translate(), "FirstSummerWarning".Translate(), LetterType.Good), null);
					}
					else if (GenDate.DaysPassed > 5)
					{
						Messages.Message("MessageSeasonBegun".Translate(new object[]
						{
							currentSeason.Label()
						}).CapitalizeFirst(), MessageSound.Standard);
					}
				}
				DateUtility.lastSeason = currentSeason;
			}
		}
	}
}
