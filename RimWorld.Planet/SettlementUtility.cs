using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class SettlementUtility
	{
		public static bool IsPlayerAttackingAnySettlementOf(Faction faction)
		{
			if (faction == Faction.OfPlayer)
			{
				return false;
			}
			if (!faction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Settlement settlement = maps[i].info.parent as Settlement;
				if (settlement != null && settlement.Faction == faction)
				{
					return true;
				}
			}
			return false;
		}
	}
}
