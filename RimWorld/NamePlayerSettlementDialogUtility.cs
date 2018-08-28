using RimWorld.Planet;
using System;

namespace RimWorld
{
	public static class NamePlayerSettlementDialogUtility
	{
		public static bool IsValidName(string s)
		{
			return s.Length != 0 && s.Length <= 64;
		}

		public static void Named(Settlement factionBase, string s)
		{
			factionBase.Name = s;
			factionBase.namedByPlayer = true;
		}
	}
}
