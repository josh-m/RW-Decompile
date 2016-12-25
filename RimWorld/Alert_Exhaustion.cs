using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Exhaustion : Alert_High
	{
		private IEnumerable<Pawn> ExhaustedColonists
		{
			get
			{
				return from p in Find.MapPawns.FreeColonistsSpawned
				where p.needs.rest != null && p.needs.rest.CurCategory == RestCategory.Exhausted
				select p;
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.ExhaustedColonists)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return string.Format("ExhaustionDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				return AlertReport.CulpritIs(this.ExhaustedColonists.FirstOrDefault<Pawn>());
			}
		}

		public Alert_Exhaustion()
		{
			this.baseLabel = "Exhaustion".Translate();
		}
	}
}
