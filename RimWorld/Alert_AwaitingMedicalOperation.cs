using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_AwaitingMedicalOperation : Alert
	{
		private IEnumerable<Pawn> AwaitingMedicalOperation
		{
			get
			{
				return from p in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer).Concat(PawnsFinder.AllMaps_PrisonersOfColonySpawned)
				where HealthAIUtility.ShouldHaveSurgeryDoneNow(p) && p.InBed()
				select p;
			}
		}

		public override string GetLabel()
		{
			return string.Format("PatientsAwaitingMedicalOperation".Translate(), this.AwaitingMedicalOperation.Count<Pawn>().ToStringCached());
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.AwaitingMedicalOperation)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return string.Format("PatientsAwaitingMedicalOperationDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return this.AwaitingMedicalOperation.FirstOrDefault<Pawn>();
		}
	}
}
