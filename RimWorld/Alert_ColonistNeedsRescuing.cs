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
				foreach (Pawn p in Find.MapPawns.FreeColonistsSpawned)
				{
					if (Alert_ColonistNeedsRescuing.NeedsRescue(p))
					{
						yield return p;
					}
				}
			}
		}

		public override string FullLabel
		{
			get
			{
				if (this.ColonistsNeedingRescue.Count<Pawn>() == 1)
				{
					return "ColonistNeedsRescue".Translate();
				}
				return "ColonistsNeedRescue".Translate();
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.ColonistsNeedingRescue)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return string.Format("ColonistsNeedRescueDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				return AlertReport.CulpritIs(this.ColonistsNeedingRescue.FirstOrDefault<Pawn>());
			}
		}

		public static bool NeedsRescue(Pawn p)
		{
			return p.Downed && !p.InBed() && p.holder == null && (p.jobs.jobQueue == null || p.jobs.jobQueue.Count <= 0 || !p.jobs.jobQueue.Peek().CanBeginNow(p));
		}
	}
}
