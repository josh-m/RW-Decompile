using System;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDefenses : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
				if (GenDate.DaysPassed < 2)
				{
					return false;
				}
				if (GenDate.DaysPassed > 5)
				{
					return false;
				}
				if (Find.ListerBuildings.allBuildingsColonist.Any((Building b) => (b.def.building != null && (b.def.building.IsTurret || b.def.building.isTrap)) || b.def == ThingDefOf.Sandbags))
				{
					return false;
				}
				return true;
			}
		}

		public Alert_NeedDefenses()
		{
			this.baseLabel = "NeedDefenses".Translate();
			this.baseExplanation = "NeedDefensesDesc".Translate();
		}
	}
}
