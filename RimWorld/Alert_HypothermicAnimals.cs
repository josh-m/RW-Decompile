using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_HypothermicAnimals : Alert_High
	{
		private IEnumerable<Pawn> HypothermicAnimals
		{
			get
			{
				return from p in Find.MapPawns.AllPawnsSpawned
				where p.RaceProps.Animal && p.Faction == null && p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia) != null
				select p;
			}
		}

		public override string FullLabel
		{
			get
			{
				return "Hypothermic wild animals (debug)";
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Debug alert:\n\nThese wild animals are hypothermic. This may indicate a bug (but it may not, if the animals are trapped or in some other wierd but legitimate situation):");
				foreach (Pawn current in this.HypothermicAnimals)
				{
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"    ",
						current,
						" at ",
						current.Position
					}));
				}
				return stringBuilder.ToString();
			}
		}

		public override AlertReport Report
		{
			get
			{
				if (!Prefs.DevMode)
				{
					return false;
				}
				return this.HypothermicAnimals.FirstOrDefault<Pawn>();
			}
		}
	}
}
