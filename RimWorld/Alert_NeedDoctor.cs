using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDoctor : Alert
	{
		private IEnumerable<Pawn> Patients
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].IsPlayerHome)
					{
						bool healthyDoc = false;
						foreach (Pawn current in maps[i].mapPawns.FreeColonistsSpawned)
						{
							if (!current.Downed && current.workSettings != null && current.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
							{
								healthyDoc = true;
								break;
							}
						}
						if (!healthyDoc)
						{
							foreach (Pawn p in maps[i].mapPawns.FreeColonistsSpawned)
							{
								if ((p.Downed && p.needs.food.CurCategory < HungerCategory.Fed && p.InBed()) || HealthAIUtility.ShouldBeTendedNow(p))
								{
									yield return p;
								}
							}
						}
					}
				}
			}
		}

		public Alert_NeedDoctor()
		{
			this.defaultLabel = "NeedDoctor".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.Patients)
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return string.Format("NeedDoctorDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			if (Find.AnyPlayerHomeMap == null)
			{
				return false;
			}
			Pawn pawn = this.Patients.FirstOrDefault<Pawn>();
			if (pawn == null)
			{
				return false;
			}
			return AlertReport.CulpritIs(pawn);
		}
	}
}
