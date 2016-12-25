using System;
using Verse;

namespace RimWorld
{
	public class Alert_NeedBatteries : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				if (Find.ListerBuildings.ColonistsHaveBuilding(ThingDefOf.Battery))
				{
					return false;
				}
				if (!Find.ListerBuildings.ColonistsHaveBuilding(ThingDefOf.SolarGenerator) && !Find.ListerBuildings.ColonistsHaveBuilding(ThingDefOf.WindTurbine))
				{
					return false;
				}
				if (Find.ListerBuildings.ColonistsHaveBuilding(ThingDefOf.GeothermalGenerator))
				{
					return false;
				}
				return true;
			}
		}

		public Alert_NeedBatteries()
		{
			this.baseLabel = "NeedBatteries".Translate();
			this.baseExplanation = "NeedBatteriesDesc".Translate();
		}
	}
}
