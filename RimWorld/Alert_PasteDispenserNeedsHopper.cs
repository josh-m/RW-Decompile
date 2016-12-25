using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_PasteDispenserNeedsHopper : Alert
	{
		public Alert_PasteDispenserNeedsHopper()
		{
			this.defaultLabel = "NeedFoodHopper".Translate();
			this.defaultExplanation = "NeedFoodHopperDesc".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				foreach (Building current in from b in maps[i].listerBuildings.allBuildingsColonist
				where b.def.IsFoodDispenser
				select b)
				{
					bool flag = false;
					ThingDef hopper = ThingDefOf.Hopper;
					foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(current))
					{
						if (current2.InBounds(maps[i]))
						{
							Thing edifice = current2.GetEdifice(current.Map);
							if (edifice != null && edifice.def == hopper)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return AlertReport.CulpritIs(current);
					}
				}
			}
			return AlertReport.Inactive;
		}
	}
}
