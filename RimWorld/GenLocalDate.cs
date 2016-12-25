using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public static class GenLocalDate
	{
		private static int TicksAbs
		{
			get
			{
				return Find.TickManager.TicksAbs;
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

		public static int DayOfMonth(Map map)
		{
			return GenLocalDate.DayOfMonth(map.Tile);
		}

		public static Month Month(Map map)
		{
			return GenLocalDate.Month(map.Tile);
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

		public static float DayPercent(Map map)
		{
			return GenLocalDate.DayPercent(map.Tile);
		}

		public static int HourInt(Map map)
		{
			return GenLocalDate.HourInt(map.Tile);
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

		public static int DayOfMonth(Thing thing)
		{
			return GenDate.DayOfMonth((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static Month Month(Thing thing)
		{
			return GenDate.Month((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static Season Season(Thing thing)
		{
			return GenDate.Season((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
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

		public static float DayPercent(Thing thing)
		{
			return GenDate.DayPercent((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
		}

		public static int HourInt(Thing thing)
		{
			return GenDate.HourInt((long)GenLocalDate.TicksAbs, GenLocalDate.LongitudeForDate(thing));
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

		public static int DayOfMonth(int tile)
		{
			return GenDate.DayOfMonth((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static Month Month(int tile)
		{
			return GenDate.Month((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static Season Season(int tile)
		{
			return GenDate.Season((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
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

		public static float DayPercent(int tile)
		{
			return GenDate.DayPercent((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		public static int HourInt(int tile)
		{
			return GenDate.HourInt((long)GenLocalDate.TicksAbs, Find.WorldGrid.LongLatOf(tile).x);
		}

		private static float LongitudeForDate(Thing thing)
		{
			Map mapHeld = thing.MapHeld;
			if (mapHeld != null)
			{
				return Find.WorldGrid.LongLatOf(mapHeld.Tile).x;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null && pawn.IsCaravanMember())
			{
				return Find.WorldGrid.LongLatOf(pawn.GetCaravan().Tile).x;
			}
			return 0f;
		}
	}
}
