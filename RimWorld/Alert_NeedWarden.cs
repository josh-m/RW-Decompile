using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_NeedWarden : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
				if (!Find.MapPawns.PrisonersOfColonySpawned.Any<Pawn>())
				{
					return AlertReport.Inactive;
				}
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (!current.Downed && current.workSettings != null && current.workSettings.GetPriority(WorkTypeDefOf.Warden) > 0)
					{
						return AlertReport.Inactive;
					}
				}
				return AlertReport.CulpritIs(Find.MapPawns.PrisonersOfColonySpawned.First<Pawn>());
			}
		}

		public Alert_NeedWarden()
		{
			this.baseLabel = "NeedWarden".Translate();
			this.baseExplanation = "NeedWardenDesc".Translate();
		}
	}
}
