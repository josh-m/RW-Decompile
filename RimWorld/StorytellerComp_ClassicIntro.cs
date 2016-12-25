using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ClassicIntro : StorytellerComp
	{
		protected int IntervalsPassed
		{
			get
			{
				return Find.TickManager.TicksGame / 1000;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents()
		{
			if (this.IntervalsPassed == 150)
			{
				yield return new FiringIncident(IncidentDefOf.VisitorGroup, this, null)
				{
					parms = 
					{
						points = (float)Rand.Range(40, 100)
					}
				};
			}
			if (this.IntervalsPassed == 204)
			{
				IncidentDef incDef;
				if ((from def in DefDatabase<IncidentDef>.AllDefs
				where def.category == IncidentCategory.ThreatSmall
				select def).TryRandomElementByWeight(new Func<IncidentDef, float>(this.IncidentChanceAdjustedForPopulation), out incDef))
				{
					yield return new FiringIncident(incDef, this, null)
					{
						parms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, incDef.category)
					};
				}
			}
			if (this.IntervalsPassed == 264)
			{
				IncidentDef inc = IncidentDefOf.WandererJoin;
				FiringIncident qi = new FiringIncident(inc, this, this.GenerateParms(inc.category));
				yield return qi;
			}
			if (this.IntervalsPassed == 324)
			{
				yield return new FiringIncident(IncidentDefOf.RaidEnemy, this, null)
				{
					parms = this.GenerateParms(IncidentDefOf.RaidEnemy.category)
				};
			}
		}
	}
}
