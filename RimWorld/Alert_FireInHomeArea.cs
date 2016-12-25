using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_FireInHomeArea : Alert_Critical
	{
		private Fire FireInHomeArea
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.Fire);
					for (int j = 0; j < list.Count; j++)
					{
						Thing thing = list[j];
						if (maps[i].areaManager.Home[thing.Position] && !thing.Position.Fogged(thing.Map))
						{
							return (Fire)thing;
						}
					}
				}
				return null;
			}
		}

		public Alert_FireInHomeArea()
		{
			this.defaultLabel = "FireInHomeArea".Translate();
			this.defaultExplanation = "FireInHomeAreaDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			Fire fireInHomeArea = this.FireInHomeArea;
			if (fireInHomeArea != null)
			{
				return AlertReport.CulpritIs(fireInHomeArea);
			}
			return false;
		}
	}
}
