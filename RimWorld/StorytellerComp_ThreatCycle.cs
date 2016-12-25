using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ThreatCycle : StorytellerComp
	{
		protected StorytellerCompProperties_ThreatCycle Props
		{
			get
			{
				return (StorytellerCompProperties_ThreatCycle)this.props;
			}
		}

		protected int QueueIntervalsPassed
		{
			get
			{
				return Find.TickManager.TicksGame / 1000;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			float curCycleDays = ((float)GenDate.DaysPassed - this.Props.minDaysPassed) % this.Props.ThreatCycleTotalDays;
			if (curCycleDays > this.Props.threatOffDays)
			{
				float daysSinceThreatBig = (float)(Find.TickManager.TicksGame - Find.StoryWatcher.storyState.LastThreatBigTick) / 60000f;
				if (daysSinceThreatBig > this.Props.minDaysBetweenThreatBigs && ((daysSinceThreatBig > this.Props.ThreatCycleTotalDays * 0.9f && curCycleDays > this.Props.ThreatCycleTotalDays * 0.95f) || Rand.MTBEventOccurs(this.Props.mtbDaysThreatBig, 60000f, 1000f)))
				{
					FiringIncident bt = this.GenerateQueuedThreatBig(target);
					if (bt != null)
					{
						yield return bt;
					}
				}
				if (Rand.MTBEventOccurs(this.Props.mtbDaysThreatSmall, 60000f, 1000f))
				{
					FiringIncident st = this.GenerateQueuedThreatSmall(target);
					if (st != null)
					{
						yield return st;
					}
				}
			}
		}

		private FiringIncident GenerateQueuedThreatSmall(IIncidentTarget target)
		{
			IncidentDef incidentDef;
			if (!(from def in DefDatabase<IncidentDef>.AllDefs
			where def.category == IncidentCategory.ThreatSmall && def.Worker.CanFireNow(target)
			select def).TryRandomElementByWeight(new Func<IncidentDef, float>(this.IncidentChanceAdjustedForPopulation), out incidentDef))
			{
				return null;
			}
			return new FiringIncident(incidentDef, this, null)
			{
				parms = this.GenerateParms(incidentDef.category, target)
			};
		}

		private FiringIncident GenerateQueuedThreatBig(IIncidentTarget target)
		{
			IncidentParms parms = this.GenerateParms(IncidentCategory.ThreatBig, target);
			IncidentDef raidEnemy;
			if (GenDate.DaysPassed < 20)
			{
				raidEnemy = IncidentDefOf.RaidEnemy;
			}
			else if (!(from def in DefDatabase<IncidentDef>.AllDefs
			where def.category == IncidentCategory.ThreatBig && parms.points >= def.minThreatPoints && def.Worker.CanFireNow(target)
			select def).TryRandomElementByWeight(new Func<IncidentDef, float>(this.IncidentChanceAdjustedForPopulation), out raidEnemy))
			{
				return null;
			}
			return new FiringIncident(raidEnemy, this, null)
			{
				parms = parms
			};
		}
	}
}
