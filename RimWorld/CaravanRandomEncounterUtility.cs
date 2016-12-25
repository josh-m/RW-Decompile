using System;
using Verse;

namespace RimWorld
{
	public static class CaravanRandomEncounterUtility
	{
		public static bool CanMeetRandomCaravanAt(int tile)
		{
			return Current.Game.FindMap(tile) == null && !Find.WorldObjects.AnyFactionBaseAt(tile);
		}
	}
}
