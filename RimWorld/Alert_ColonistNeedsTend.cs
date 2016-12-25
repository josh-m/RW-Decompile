using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistNeedsTend : Alert_High
	{
		private IEnumerable<Pawn> NeedingColonists
		{
			get
			{
				foreach (Pawn p in Find.MapPawns.FreeColonistsSpawned)
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

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.NeedingColonists)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return string.Format("ColonistNeedsTreatmentDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				Pawn pawn = this.NeedingColonists.FirstOrDefault<Pawn>();
				if (pawn == null)
				{
					return false;
				}
				return AlertReport.CulpritIs(pawn);
			}
		}

		public Alert_ColonistNeedsTend()
		{
			this.baseLabel = "ColonistNeedsTreatment".Translate();
		}
	}
}
