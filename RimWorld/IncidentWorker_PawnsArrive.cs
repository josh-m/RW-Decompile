using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_PawnsArrive : IncidentWorker
	{
		protected IEnumerable<Faction> CandidateFactions(bool desperate = false)
		{
			return from f in Find.FactionManager.AllFactions
			where this.FactionCanBeGroupSource(f, desperate)
			select f;
		}

		protected virtual bool FactionCanBeGroupSource(Faction f, bool desperate = false)
		{
			return !f.IsPlayer && (desperate || (f.def.allowedArrivalTemperatureRange.Includes(GenTemperature.OutdoorTemp) && f.def.allowedArrivalTemperatureRange.Includes(GenTemperature.SeasonalTemp)));
		}

		protected override bool CanFireNowSub()
		{
			return this.CandidateFactions(false).Any<Faction>();
		}

		public string DebugListingOfGroupSources()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				stringBuilder.Append(current.Name);
				if (this.FactionCanBeGroupSource(current, false))
				{
					stringBuilder.Append("    YES");
				}
				else if (this.FactionCanBeGroupSource(current, true))
				{
					stringBuilder.Append("    YES-DESPERATE");
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}
