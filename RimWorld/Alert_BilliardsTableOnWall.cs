using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_BilliardsTableOnWall : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				Thing thing = this.BadTable();
				if (thing == null)
				{
					return false;
				}
				return AlertReport.CulpritIs(thing);
			}
		}

		public Alert_BilliardsTableOnWall()
		{
			this.baseLabel = "BilliardsNeedsSpace".Translate();
			this.baseExplanation = "BilliardsNeedsSpaceDesc".Translate();
		}

		private Thing BadTable()
		{
			List<Thing> list = Find.ListerThings.ThingsOfDef(ThingDefOf.BilliardsTable);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Faction == Faction.OfPlayer && !JoyGiver_PlayBilliards.ThingHasStandableSpaceOnAllSides(list[i]))
				{
					return list[i];
				}
			}
			return null;
		}
	}
}
