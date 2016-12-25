using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDefenses : Alert
	{
		public Alert_NeedDefenses()
		{
			this.defaultLabel = "NeedDefenses".Translate();
			this.defaultExplanation = "NeedDefensesDesc".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			if (GenDate.DaysPassed < 2 || GenDate.DaysPassed > 5)
			{
				return false;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (this.NeedDefenses(maps[i]))
				{
					return true;
				}
			}
			return false;
		}

		private bool NeedDefenses(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			return !map.listerBuildings.allBuildingsColonist.Any((Building b) => (b.def.building != null && (b.def.building.IsTurret || b.def.building.isTrap)) || b.def == ThingDefOf.Sandbags);
		}
	}
}
