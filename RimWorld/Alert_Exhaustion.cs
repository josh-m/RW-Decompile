using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Exhaustion : Alert
	{
		private IEnumerable<Pawn> ExhaustedColonists
		{
			get
			{
				return from p in PawnsFinder.AllMaps_FreeColonistsSpawned
				where p.needs.rest != null && p.needs.rest.CurCategory == RestCategory.Exhausted
				select p;
			}
		}

		public Alert_Exhaustion()
		{
			this.defaultLabel = "Exhaustion".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.ExhaustedColonists)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return string.Format("ExhaustionDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritIs(this.ExhaustedColonists.FirstOrDefault<Pawn>());
		}
	}
}
