using RimWorld.Planet;
using System;

namespace RimWorld
{
	public static class NamePlayerFactionBaseDialogUtility
	{
		public static bool IsValidName(string s)
		{
			return s.Length != 0;
		}

		public static void Named(FactionBase factionBase, string s)
		{
			factionBase.Name = s;
			factionBase.namedByPlayer = true;
		}
	}
}
