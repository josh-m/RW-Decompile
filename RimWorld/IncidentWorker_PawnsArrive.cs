using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_PawnsArrive : IncidentWorker
	{
		protected IEnumerable<Faction> CandidateFactions(Map map, bool desperate = false)
		{
			return from f in Find.FactionManager.AllFactions
			where this.FactionCanBeGroupSource(f, map, desperate)
			select f;
		}

		protected virtual bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return !f.IsPlayer && !f.defeated && (desperate || (f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) && f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)));
		}

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return this.CandidateFactions(map, false).Any<Faction>();
		}

		public string DebugListingOfGroupSources()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				stringBuilder.Append(current.Name);
				if (this.FactionCanBeGroupSource(current, Find.VisibleMap, false))
				{
					stringBuilder.Append("    YES");
				}
				else if (this.FactionCanBeGroupSource(current, Find.VisibleMap, true))
				{
					stringBuilder.Append("    YES-DESPERATE");
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}
