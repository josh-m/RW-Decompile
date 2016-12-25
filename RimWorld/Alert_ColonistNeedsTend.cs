using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistNeedsTend : Alert
	{
		private IEnumerable<Pawn> NeedingColonists
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (p.health.HasHediffsNeedingTendByColony(true))
					{
						Building_Bed curBed = p.CurrentBed();
						if (curBed == null || !curBed.Medical)
						{
							if (!Alert_ColonistNeedsRescuing.NeedsRescue(p))
							{
								yield return p;
							}
						}
					}
				}
			}
		}

		public Alert_ColonistNeedsTend()
		{
			this.defaultLabel = "ColonistNeedsTreatment".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.NeedingColonists)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return string.Format("ColonistNeedsTreatmentDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			Pawn pawn = this.NeedingColonists.FirstOrDefault<Pawn>();
			if (pawn == null)
			{
				return false;
			}
			return AlertReport.CulpritIs(pawn);
		}
	}
}
