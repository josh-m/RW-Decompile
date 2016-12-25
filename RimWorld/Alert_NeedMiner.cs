using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_NeedMiner : Alert_High
	{
		public override AlertReport Report
		{
			get
			{
				Designation designation = (from d in Find.DesignationManager.allDesignations
				where d.def == DesignationDefOf.Mine
				select d).FirstOrDefault<Designation>();
				if (designation == null)
				{
					return AlertReport.Inactive;
				}
				foreach (Pawn current in Find.MapPawns.FreeColonistsSpawned)
				{
					if (!current.Downed && current.workSettings != null && current.workSettings.GetPriority(WorkTypeDefOf.Mining) > 0)
					{
						return AlertReport.Inactive;
					}
				}
				return AlertReport.CulpritIs(designation.target.Thing);
			}
		}

		public Alert_NeedMiner()
		{
			this.baseLabel = "NeedMiner".Translate();
			this.baseExplanation = "NeedMinerDesc".Translate();
		}
	}
}
