using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class FactionBaseUtility
	{
		public static bool IsPlayerAttackingAnyFactionBaseOf(Faction faction)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				FactionBase factionBase = maps[i].info.parent as FactionBase;
				if (factionBase != null && factionBase.Faction == faction)
				{
					return true;
				}
			}
			return false;
		}
	}
}
