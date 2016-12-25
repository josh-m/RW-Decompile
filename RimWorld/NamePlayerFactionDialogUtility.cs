using System;
using Verse;

namespace RimWorld
{
	public static class NamePlayerFactionDialogUtility
	{
		public static bool IsValidName(string s)
		{
			return s.Length != 0 && GenText.IsValidFilename(s);
		}

		public static void Named(string s)
		{
			Faction.OfPlayer.Name = s;
		}
	}
}
