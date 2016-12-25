using System;
using Verse;

namespace RimWorld
{
	public class Alert_NeedJoySource : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				if (GenDate.DaysPassedFloat < 6.5f)
				{
					return false;
				}
				if (Find.ListerBuildings.allBuildingsColonist.Any((Building b) => b.def.building.isJoySource))
				{
					return false;
				}
				return true;
			}
		}

		public Alert_NeedJoySource()
		{
			this.baseLabel = "NeedJoySource".Translate();
			this.baseExplanation = "NeedJoySourceDesc".Translate();
		}
	}
}
