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
				foreach (Pawn p in Find.MapPawns.FreeColonistsSpawned)
				{
					if (!p.SafeTemperatureRange().Includes(GenTemperature.GetTemperatureForCell(p.Position)))
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

		public override string FullExplanation
		{
			get
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
		}

		public override AlertReport Report
		{
			get
			{
				Pawn pawn = this.HypothermiaDangerColonists.FirstOrDefault<Pawn>();
				if (pawn == null)
				{
					return false;
				}
				return AlertReport.CulpritIs(pawn);
			}
		}

		public Alert_Hypothermia()
		{
			this.baseLabel = "AlertHypothermia".Translate();
		}
	}
}
