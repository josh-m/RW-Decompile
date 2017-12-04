using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLocalDate
	{
		private static int TicksAbs
		{
			get
			{
				return GenTicks.TicksAbs;
			}
		}

		public static int DayOfYear(Map map)
		{
			return GenLocalDate.DayOfYear(map.Tile);
		}

		public static int HourOfDay(Map map)
		{
			return GenLocalDate.HourOfDay(map.Tile);
		}

		public static int DayOfTwelfth(Map map)
		{
			return GenLocalDate.DayOfTwelfth(map.Tile);
		}

		public static Twelfth Twelfth(Map map)
		{
			return GenLocalDate.Twelfth(map.Tile);
		}

		public static Season Season(Map map)
		{
			return GenLocalDate.Season(map.Tile);
		}

		public static int Year(Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 5500;
			}
			return GenLocalDate.Year(map.Tile);
		}

		public static int DayOfSeason(Map map)
		{
			return GenLocalDate.DayOfSeason(map.Tile);
		}

		public static int DayOfQuadrum(Map map)
		{
			return GenLocalDate.DayOfQuadrum(map.Tile);
		}

		public static float DayPercent(Map map)
		{
			return GenLocalDate.DayPercent(map.Tile);
		}

		public static float YearPercent(Map map)
		{
			return GenLocalDate.YearPercent(map.Tile);
		}

		public static int HourInteger(Map map)
		{
			return GenLocalDate.HourInteger(map.Tile);
		}

		public static float HourFloat(Map map)
		{
			return GenLocalDate.HourFloat(map.Tile);
		}

		public static int DayOfYear(Thing thing)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				return GenDate.DayOfYear((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
			}
			return 0;
		}

		public static int HourOfDay(Thing thing)
		{
			return GenDate.HourOfDay((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static int DayOfTwelfth(Thing thing)
		{
			return GenDate.DayOfTwelfth((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static Twelfth Twelfth(Thing thing)
		{
			return GenDate.Twelfth((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static Season Season(Thing thing)
		{
			return GenDate.Season((long)GenLocalDate.TicksAbs, GenLocalDate.LocationForDate(thing));
		}

		public static int Year(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 5500;
			}
			return GenDate.Year((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static int DayOfSeason(Thing thing)
		{
			return GenDate.DayOfSeason((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static int DayOfQuadrum(Thing thing)
		{
			return GenDate.DayOfQuadrum((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static float DayPercent(Thing thing)
		{
			return GenDate.DayPercent((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static float YearPercent(Thing thing)
		{
			return GenDate.YearPercent((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static int HourInteger(Thing thing)
		{
			return GenDate.HourInteger((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static float HourFloat(Thing thing)
		{
			return GenDate.HourFloat((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static int DayOfYear(int tile)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				return GenDate.DayOfYear((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
			}
			return 0;
		}

		public static int HourOfDay(int tile)
		{
			return GenDate.HourOfDay((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayOfTwelfth(int tile)
		{
			return GenDate.DayOfTwelfth((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static Twelfth Twelfth(int tile)
		{
			return GenDate.Twelfth((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static Season Season(int tile)
		{
			return GenDate.Season((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile));
		}

		public static int Year(int tile)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return 5500;
			}
			return GenDate.Year((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayOfSeason(int tile)
		{
			return GenDate.DayOfSeason((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int DayOfQuadrum(int tile)
		{
			return GenDate.DayOfQuadrum((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static float DayPercent(int tile)
		{
			return GenDate.DayPercent((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static float YearPercent(int tile)
		{
			return GenDate.YearPercent((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int HourInteger(int tile)
		{
			return GenDate.HourInteger((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static float HourFloat(int tile)
		{
			return GenDate.HourFloat((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		private static float LongitudeForDate(Thing thing)
		{
			return GenLocalDate.LocationForDate(thing).x;
		}

		private static Vector2 LocationForDate(Thing thing)
		{
			int tile = thing.Tile;
			if (tile >= 0)
			{
				return Find.WorldGrid.LongLatOf(tile);
			}
			return Vector2.zero;
		}
	}
}
