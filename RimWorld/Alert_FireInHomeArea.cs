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
				List<Thing> list = Find.ListerThings.ThingsOfDef(ThingDefOf.Fire);
				for (int i = 0; i < list.Count; i++)
				{
					if (Find.AreaHome[list[i].Position])
					{
						return (Fire)list[i];
					}
				}
				return null;
			}
		}

		public override string FullLabel
		{
			get
			{
				return "FireInHomeArea".Translate();
			}
		}

		public override string FullExplanation
		{
			get
			{
				return "FireInHomeAreaDesc".Translate();
			}
		}

		public override AlertReport Report
		{
			get
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
}
