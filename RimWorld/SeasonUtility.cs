using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class SeasonUtility
	{
		public static Season FirstSeason
		{
			get
			{
				return Season.Spring;
			}
		}

		public static Twelfth GetFirstTwelfth(this Season season, float latitude)
		{
			if (latitude >= 0f)
			{
				switch (season)
				{
				case Season.Spring:
					return Twelfth.First;
				case Season.Summer:
					return Twelfth.Fourth;
				case Season.Fall:
					return Twelfth.Seventh;
				case Season.Winter:
					return Twelfth.Tenth;
				}
			}
			else
			{
				switch (season)
				{
				case Season.Spring:
					return Twelfth.Seventh;
				case Season.Summer:
					return Twelfth.Tenth;
				case Season.Fall:
					return Twelfth.First;
				case Season.Winter:
					return Twelfth.Fourth;
				}
			}
			return Twelfth.Undefined;
		}

		public static Twelfth GetMiddleTwelfth(this Season season, float latitude)
		{
			if (latitude >= 0f)
			{
				switch (season)
				{
				case Season.Spring:
					return Twelfth.Second;
				case Season.Summer:
					return Twelfth.Fifth;
				case Season.Fall:
					return Twelfth.Eighth;
				case Season.Winter:
					return Twelfth.Eleventh;
				}
			}
			else
			{
				switch (season)
				{
				case Season.Spring:
					return Twelfth.Eighth;
				case Season.Summer:
					return Twelfth.Eleventh;
				case Season.Fall:
					return Twelfth.Second;
				case Season.Winter:
					return Twelfth.Fifth;
				}
			}
			return Twelfth.Undefined;
		}

		public static Season GetPreviousSeason(this Season season)
		{
			switch (season)
			{
			case Season.Undefined:
				return Season.Undefined;
			case Season.Spring:
				return Season.Winter;
			case Season.Summer:
				return Season.Spring;
			case Season.Fall:
				return Season.Summer;
			case Season.Winter:
				return Season.Fall;
			default:
				return Season.Undefined;
			}
		}

		public static float GetMiddleYearPct(this Season season, float latitude)
		{
			if (season == Season.Undefined)
			{
				return 0.5f;
			}
			Twelfth middleTwelfth = season.GetMiddleTwelfth(latitude);
			return ((float)middleTwelfth + 0.5f) / 12f;
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

		public static string SeasonsRangeLabel(List<Twelfth> twelfths, Vector2 longLat)
		{
			if (twelfths.Count == 0)
			{
				return string.Empty;
			}
			if (twelfths.Count == 12)
			{
				return "WholeYear".Translate();
			}
			string text = string.Empty;
			for (int i = 0; i < 12; i++)
			{
				Twelfth twelfth = (Twelfth)i;
				if (twelfths.Contains(twelfth))
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += SeasonUtility.SeasonsContinuousRangeLabel(twelfths, twelfth, longLat);
				}
			}
			return text;
		}

		private static string SeasonsContinuousRangeLabel(List<Twelfth> twelfths, Twelfth rootTwelfth, Vector2 longLat)
		{
			Twelfth leftMostTwelfth = TwelfthUtility.GetLeftMostTwelfth(twelfths, rootTwelfth);
			Twelfth rightMostTwelfth = TwelfthUtility.GetRightMostTwelfth(twelfths, rootTwelfth);
			for (Twelfth twelfth = leftMostTwelfth; twelfth != rightMostTwelfth; twelfth = TwelfthUtility.TwelfthAfter(twelfth))
			{
				if (!twelfths.Contains(twelfth))
				{
					Log.Error(string.Concat(new object[]
					{
						"Twelfths doesn't contain ",
						twelfth,
						" (",
						leftMostTwelfth,
						"..",
						rightMostTwelfth,
						")"
					}));
					break;
				}
				twelfths.Remove(twelfth);
			}
			twelfths.Remove(rightMostTwelfth);
			return GenDate.SeasonDateStringAt(leftMostTwelfth, longLat) + " - " + GenDate.SeasonDateStringAt(rightMostTwelfth, longLat);
		}
	}
}
