using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RimWorld
{
	public class ScenPart_DisableIncident : ScenPart_IncidentBase
	{
		protected override string IncidentTag
		{
			get
			{
				return "DisableIncident";
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<IncidentDef> RandomizableIncidents()
		{
			yield return IncidentDefOf.TraderCaravanArrival;
			yield return IncidentDefOf.OrbitalTraderArrival;
			yield return IncidentDefOf.WandererJoin;
			yield return IncidentDefOf.Eclipse;
			yield return IncidentDefOf.ToxicFallout;
			yield return IncidentDefOf.SolarFlare;
		}
	}
}
