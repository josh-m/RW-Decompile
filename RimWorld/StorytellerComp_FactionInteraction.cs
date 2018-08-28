using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_FactionInteraction : StorytellerComp
	{
		private StorytellerCompProperties_FactionInteraction Props
		{
			get
			{
				return (StorytellerCompProperties_FactionInteraction)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (this.Props.minDanger != StoryDanger.None)
			{
				Map map = target as Map;
				if (map == null || map.dangerWatcher.DangerRating < this.Props.minDanger)
				{
					return;
				}
			}
			float allyIncidentFraction = StorytellerUtility.AllyIncidentFraction(this.Props.fullAlliesOnly);
			if (allyIncidentFraction > 0f)
			{
				int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), this.Props.minDaysPassed, 60f, 0f, this.Props.minSpacingDays, this.Props.baseIncidentsPerYear, this.Props.baseIncidentsPerYear, allyIncidentFraction);
				for (int i = 0; i < incCount; i++)
				{
					IncidentParms parms = this.GenerateParms(this.Props.incident.category, target);
					if (this.Props.incident.Worker.CanFireNow(parms, false))
					{
						yield return new FiringIncident(this.Props.incident, this, parms);
					}
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " (" + this.Props.incident.defName + ")";
		}
	}
}
