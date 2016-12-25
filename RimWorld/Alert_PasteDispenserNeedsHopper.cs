using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_PasteDispenserNeedsHopper : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
				foreach (Building current in from b in Find.ListerBuildings.allBuildingsColonist
				where b.def.IsFoodDispenser
				select b)
				{
					bool flag = false;
					ThingDef hopper = ThingDefOf.Hopper;
					foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(current))
					{
						Thing edifice = current2.GetEdifice();
						if (edifice != null && edifice.def == hopper)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return AlertReport.CulpritIs(current);
					}
				}
				return AlertReport.Inactive;
			}
		}

		public Alert_PasteDispenserNeedsHopper()
		{
			this.baseLabel = "NeedFoodHopper".Translate();
			this.baseExplanation = "NeedFoodHopperDesc".Translate();
		}
	}
}
