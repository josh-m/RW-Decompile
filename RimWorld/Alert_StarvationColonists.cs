using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationColonists : Alert
	{
		private IEnumerable<Pawn> StarvingColonists
		{
			get
			{
				return from p in PawnsFinder.AllMaps_FreeColonistsSpawned
				where p.needs.food != null && p.needs.food.Starving
				select p;
			}
		}

		public Alert_StarvationColonists()
		{
			this.defaultLabel = "Starvation".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.StarvingColonists)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return string.Format("StarvationDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritIs(this.StarvingColonists.FirstOrDefault<Pawn>());
		}
	}
}
