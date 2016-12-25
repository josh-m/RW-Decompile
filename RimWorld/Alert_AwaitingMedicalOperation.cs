using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_AwaitingMedicalOperation : Alert_Medium
	{
		private IEnumerable<Pawn> AwaitingMedicalOperation
		{
			get
			{
				return from p in Find.MapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Concat(Find.MapPawns.PrisonersOfColonySpawned)
				where p.health.ShouldDoSurgeryNow && p.InBed()
				select p;
			}
		}

		public override string FullLabel
		{
			get
			{
				return string.Format("PatientsAwaitingMedicalOperation".Translate(), this.AwaitingMedicalOperation.Count<Pawn>().ToStringCached());
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.AwaitingMedicalOperation)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return string.Format("PatientsAwaitingMedicalOperationDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				return this.AwaitingMedicalOperation.FirstOrDefault<Pawn>();
			}
		}
	}
}
