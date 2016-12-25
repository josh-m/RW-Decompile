using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_BilliardsTableOnWall : Alert
	{
		public Alert_BilliardsTableOnWall()
		{
			this.defaultLabel = "BilliardsNeedsSpace".Translate();
			this.defaultExplanation = "BilliardsNeedsSpaceDesc".Translate();
		}

		private Thing BadTable()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.BilliardsTable);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].Faction == Faction.OfPlayer && !JoyGiver_PlayBilliards.ThingHasStandableSpaceOnAllSides(list[j]))
					{
						return list[j];
					}
				}
			}
			return null;
		}

		public override AlertReport GetReport()
		{
			Thing thing = this.BadTable();
			if (thing == null)
			{
				return false;
			}
			return AlertReport.CulpritIs(thing);
		}
	}
}
