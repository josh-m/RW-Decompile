using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedResearchProject : Alert
	{
		public Alert_NeedResearchProject()
		{
			this.defaultLabel = "NeedResearchProject".Translate();
			this.defaultExplanation = "NeedResearchProjectDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (Find.AnyPlayerHomeMap == null)
			{
				return false;
			}
			if (Find.ResearchManager.currentProj != null)
			{
				return false;
			}
			bool flag = false;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome)
				{
					if (maps[i].listerBuildings.ColonistsHaveResearchBench())
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			if (!Find.ResearchManager.AnyProjectIsAvailable)
			{
				return false;
			}
			return true;
		}
	}
}
