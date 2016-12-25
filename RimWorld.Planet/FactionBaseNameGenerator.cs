using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class FactionBaseNameGenerator
	{
		private static List<string> usedNames = new List<string>();

		public static string GenerateFactionBaseName(FactionBase factionBase)
		{
			if (factionBase.Faction == null || factionBase.Faction.def.factionBaseNameMaker == null)
			{
				return factionBase.def.label;
			}
			FactionBaseNameGenerator.usedNames.Clear();
			List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
			for (int i = 0; i < factionBases.Count; i++)
			{
				FactionBase factionBase2 = factionBases[i];
				if (factionBase2.Name != null)
				{
					FactionBaseNameGenerator.usedNames.Add(factionBase2.Name);
				}
			}
			return NameGenerator.GenerateName(factionBase.Faction.def.factionBaseNameMaker, FactionBaseNameGenerator.usedNames, true);
		}
	}
}
