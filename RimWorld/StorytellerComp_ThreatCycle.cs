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
		public override IEnumerable<FiringIncident> MakeIntervalIncidents()
		{
			float threatCycleDays = ((float)GenDate.DaysPassed - this.Props.minDaysPassed) % this.Props.threatCycleLength;
			float threatCyclePos = threatCycleDays / this.Props.threatCycleLength;
			if (threatCycleDays > 0f && threatCyclePos > 0.5f)
			{
				int ticksSinceThreatBig = Find.TickManager.TicksGame - Find.StoryWatcher.storyState.LastThreatBigTick;
				if ((float)ticksSinceThreatBig > this.Props.minDaysBetweenThreatBigs * 60000f && (((double)ticksSinceThreatBig > (double)this.Props.threatCycleLength * 0.8 && threatCyclePos > 0.85f) || Rand.MTBEventOccurs(this.Props.mtbDaysThreatBig, 60000f, 1000f)))
				{
					FiringIncident bt = this.GenerateQueuedThreatBig();
					if (bt != null)
					{
						yield return bt;
					}
				}
				if (GenDate.DaysPassed > 8 && Rand.MTBEventOccurs(this.Props.mtbDaysThreatSmall, 60000f, 1000f))
				{
					FiringIncident st = this.GenerateQueuedThreatSmall();
					if (st != null)
					{
						yield return st;
					}
				}
			}
		}

		private FiringIncident GenerateQueuedThreatSmall()
		{
			IncidentDef incidentDef;
			if (!(from def in DefDatabase<IncidentDef>.AllDefs
			where def.category == IncidentCategory.ThreatSmall && def.Worker.CanFireNow()
			select def).TryRandomElementByWeight(new Func<IncidentDef, float>(this.IncidentChanceAdjustedForPopulation), out incidentDef))
			{
				return null;
			}
			return new FiringIncident(incidentDef, this, null)
			{
				parms = this.GenerateParms(incidentDef.category)
			};
		}

		private FiringIncident GenerateQueuedThreatBig()
		{
			IncidentParms parms = this.GenerateParms(IncidentCategory.ThreatBig);
			IncidentDef raidEnemy;
			if (GenDate.DaysPassed < 20)
			{
				raidEnemy = IncidentDefOf.RaidEnemy;
			}
			else if (!(from def in DefDatabase<IncidentDef>.AllDefs
			where def.category == IncidentCategory.ThreatBig && parms.points >= def.minThreatPoints && def.Worker.CanFireNow()
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
