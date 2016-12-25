using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedJoySource : Alert
	{
		public Alert_NeedJoySource()
		{
			this.defaultLabel = "NeedJoySource".Translate();
			this.defaultExplanation = "NeedJoySourceDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (GenDate.DaysPassedFloat < 6.5f)
			{
				return false;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (this.NeedJoySource(maps[i]))
				{
					return true;
				}
			}
			return false;
		}

		private bool NeedJoySource(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			return !map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.building.isJoySource);
		}
	}
}
