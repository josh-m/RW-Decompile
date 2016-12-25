using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationColonists : Alert_High
	{
		private IEnumerable<Pawn> StarvingColonists
		{
			get
			{
				return from p in Find.MapPawns.FreeColonistsSpawned
				where p.needs.food != null && p.needs.food.Starving
				select p;
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.StarvingColonists)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return string.Format("StarvationDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				return AlertReport.CulpritIs(this.StarvingColonists.FirstOrDefault<Pawn>());
			}
		}

		public Alert_StarvationColonists()
		{
			this.baseLabel = "Starvation".Translate();
		}
	}
}
