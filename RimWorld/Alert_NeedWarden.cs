using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_NeedWarden : Alert
	{
		public Alert_NeedWarden()
		{
			this.defaultLabel = "NeedWarden".Translate();
			this.defaultExplanation = "NeedWardenDesc".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
				{
					if (map.mapPawns.PrisonersOfColonySpawned.Any<Pawn>())
					{
						bool flag = false;
						foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
						{
							if (!current.Downed && current.workSettings != null && current.workSettings.GetPriority(WorkTypeDefOf.Warden) > 0)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							return AlertReport.CulpritIs(map.mapPawns.PrisonersOfColonySpawned.First<Pawn>());
						}
					}
				}
			}
			return AlertReport.Inactive;
		}
	}
}
