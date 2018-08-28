using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_OnOffCycle : StorytellerComp
	{
		protected StorytellerCompProperties_OnOffCycle Props
		{
			get
			{
				return (StorytellerCompProperties_OnOffCycle)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			float difficultyFactor = (!this.Props.applyRaidBeaconThreatMtbFactor) ? 1f : Find.Storyteller.difficulty.raidBeaconThreatCountFactor;
			float acceptFraction = 1f;
			if (this.Props.acceptFractionByDaysPassedCurve != null)
			{
				acceptFraction *= this.Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
			}
			if (this.Props.acceptPercentFactorPerThreatPointsCurve != null)
			{
				acceptFraction *= this.Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target));
			}
			int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), this.Props.minDaysPassed, this.Props.onDays, this.Props.offDays, this.Props.minSpacingDays, this.Props.numIncidentsRange.min * difficultyFactor, this.Props.numIncidentsRange.max * difficultyFactor, acceptFraction);
			for (int i = 0; i < incCount; i++)
			{
				FiringIncident fi = this.GenerateIncident(target);
				if (fi != null)
				{
					yield return fi;
				}
			}
		}

		private FiringIncident GenerateIncident(IIncidentTarget target)
		{
			IncidentParms parms = this.GenerateParms(this.Props.IncidentCategory, target);
			IncidentDef def2;
			if ((float)GenDate.DaysPassed < this.Props.forceRaidEnemyBeforeDaysPassed)
			{
				if (!IncidentDefOf.RaidEnemy.Worker.CanFireNow(parms, false))
				{
					return null;
				}
				def2 = IncidentDefOf.RaidEnemy;
			}
			else if (this.Props.incident != null)
			{
				if (!this.Props.incident.Worker.CanFireNow(parms, false))
				{
					return null;
				}
				def2 = this.Props.incident;
			}
			else if (!(from def in base.UsableIncidentsInCategory(this.Props.IncidentCategory, parms)
			where parms.points >= def.minThreatPoints
			select def).TryRandomElementByWeight(new Func<IncidentDef, float>(base.IncidentChanceFinal), out def2))
			{
				return null;
			}
			return new FiringIncident(def2, this, null)
			{
				parms = parms
			};
		}

		public override string ToString()
		{
			return base.ToString() + " (" + ((this.Props.incident == null) ? this.Props.IncidentCategory.defName : this.Props.incident.defName) + ")";
		}
	}
}
