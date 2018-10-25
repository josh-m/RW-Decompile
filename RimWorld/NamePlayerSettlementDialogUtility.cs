using RimWorld.Planet;
using System;
using Verse.Grammar;

namespace RimWorld
{
	public static class NamePlayerSettlementDialogUtility
	{
		public static bool IsValidName(string s)
		{
			return s.Length != 0 && s.Length <= 64 && !GrammarResolver.ContainsSpecialChars(s);
		}

		public static void Named(Settlement factionBase, string s)
		{
			factionBase.Name = s;
			factionBase.namedByPlayer = true;
		}
	}
}
