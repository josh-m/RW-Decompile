using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationAnimals : Alert_Medium
	{
		private IEnumerable<Pawn> StarvingAnimals
		{
			get
			{
				return from p in Find.MapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
				where p.HostFaction == null && !p.RaceProps.Humanlike
				where p.needs.food != null && p.needs.food.TicksStarving > 30000
				select p;
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in from a in this.StarvingAnimals
				orderby a.def.label
				select a)
				{
					stringBuilder.Append("    " + current.NameStringShort);
					if (current.Name.IsValid && !current.Name.Numerical)
					{
						stringBuilder.Append(" (" + current.def.label + ")");
					}
					stringBuilder.AppendLine();
				}
				return string.Format("StarvationAnimalsDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				return AlertReport.CulpritIs(this.StarvingAnimals.FirstOrDefault<Pawn>());
			}
		}

		public Alert_StarvationAnimals()
		{
			this.baseLabel = "StarvationAnimals".Translate();
		}
	}
}
