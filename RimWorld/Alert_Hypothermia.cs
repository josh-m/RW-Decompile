using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Hypothermia : Alert_Critical
	{
		private IEnumerable<Pawn> HypothermiaDangerColonists
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (!p.SafeTemperatureRange().Includes(GenTemperature.GetTemperatureForCell(p.Position, p.Map)))
					{
						Hediff hypo = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
						if (hypo != null && hypo.CurStageIndex >= 3)
						{
							yield return p;
						}
					}
				}
			}
		}

		public Alert_Hypothermia()
		{
			this.defaultLabel = "AlertHypothermia".Translate();
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.HypothermiaDangerColonists)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return "AlertHypothermiaDesc".Translate(new object[]
			{
				stringBuilder.ToString()
			});
		}

		public override AlertReport GetReport()
		{
			Pawn pawn = this.HypothermiaDangerColonists.FirstOrDefault<Pawn>();
			if (pawn == null)
			{
				return false;
			}
			return AlertReport.CulpritIs(pawn);
		}
	}
}
