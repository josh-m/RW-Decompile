using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_NeedMiner : Alert
	{
		public Alert_NeedMiner()
		{
			this.defaultLabel = "NeedMiner".Translate();
			this.defaultExplanation = "NeedMinerDesc".Translate();
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
					Designation designation = (from d in map.designationManager.allDesignations
					where d.def == DesignationDefOf.Mine
					select d).FirstOrDefault<Designation>();
					if (designation != null)
					{
						bool flag = false;
						foreach (Pawn current in map.mapPawns.FreeColonistsSpawned)
						{
							if (!current.Downed && current.workSettings != null && current.workSettings.GetPriority(WorkTypeDefOf.Mining) > 0)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							return AlertReport.CulpritIs(designation.target.Thing);
						}
					}
				}
			}
			return AlertReport.Inactive;
		}
	}
}
