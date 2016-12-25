using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_LifeThreateningHediff : Alert_Critical
	{
		private IEnumerable<Pawn> SickPawns
		{
			get
			{
				foreach (Pawn p in Find.MapPawns.FreeColonistsAndPrisonersSpawned)
				{
					foreach (Hediff diff in p.health.hediffSet.hediffs)
					{
						if (diff.CurStage != null && diff.CurStage.lifeThreatening && !diff.FullyImmune())
						{
							yield return p;
						}
					}
				}
			}
		}

		public override string FullLabel
		{
			get
			{
				return "PawnsWithLifeThreateningDisease".Translate();
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				foreach (Pawn current in this.SickPawns)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
					foreach (Hediff current2 in current.health.hediffSet.hediffs)
					{
						if (current2.CurStage != null && current2.CurStage.lifeThreatening && current2.Part != null && current2.Part != current.RaceProps.body.corePart)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					return string.Format("PawnsWithLifeThreateningDiseaseAmputationDesc".Translate(), stringBuilder.ToString());
				}
				return string.Format("PawnsWithLifeThreateningDiseaseDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				return AlertReport.CulpritIs(this.SickPawns.FirstOrDefault<Pawn>());
			}
		}
	}
}
