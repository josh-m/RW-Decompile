using System;
using Verse;

namespace RimWorld
{
	public class Alert_NeedMealSource : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				if (GenDate.DaysPassed < 2)
				{
					return false;
				}
				if (Find.ListerBuildings.allBuildingsColonist.Any((Building b) => b.def.building.isMealSource))
				{
					return false;
				}
				return true;
			}
		}

		public Alert_NeedMealSource()
		{
			this.baseLabel = "NeedMealSource".Translate();
			this.baseExplanation = "NeedMealSourceDesc".Translate();
		}
	}
}
