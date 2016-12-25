using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistNeedsRescuing : Alert_Critical
	{
		private IEnumerable<Pawn> ColonistsNeedingRescue
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (Alert_ColonistNeedsRescuing.NeedsRescue(p))
					{
						yield return p;
					}
				}
			}
		}

		public static bool NeedsRescue(Pawn p)
		{
			return p.Downed && !p.InBed() && p.holdingContainer == null && (p.jobs.jobQueue == null || p.jobs.jobQueue.Count <= 0 || !p.jobs.jobQueue.Peek().CanBeginNow(p));
		}

		public override string GetLabel()
		{
			if (this.ColonistsNeedingRescue.Count<Pawn>() == 1)
			{
				return "ColonistNeedsRescue".Translate();
			}
			return "ColonistsNeedRescue".Translate();
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.ColonistsNeedingRescue)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return string.Format("ColonistsNeedRescueDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritIs(this.ColonistsNeedingRescue.FirstOrDefault<Pawn>());
		}
	}
}
