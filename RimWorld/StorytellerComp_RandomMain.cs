using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_RandomMain : StorytellerComp
	{
		protected StorytellerCompProperties_RandomMain Props
		{
			get
			{
				return (StorytellerCompProperties_RandomMain)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents()
		{
			if (Rand.MTBEventOccurs(this.Props.incidentMtb, 60000f, 1000f))
			{
				IncidentCategory category = this.DecideCategory();
				IncidentParms parms = this.GenerateParms(category);
				IEnumerable<IncidentDef> options = from d in DefDatabase<IncidentDef>.AllDefs
				where d.category == this.<category>__0 && d.Worker.CanFireNow() && (!d.NeedsParms || d.minThreatPoints <= this.<parms>__1.points)
				select d;
				IncidentDef incDef;
				if (options.TryRandomElementByWeight(new Func<IncidentDef, float>(this.IncidentChanceAdjustedForPopulation), out incDef))
				{
					yield return new FiringIncident(incDef, this, this.GenerateParms(incDef.category));
				}
			}
		}

		protected override float IncidentChanceAdjustedForPopulation(IncidentDef def)
		{
			float num = 1f;
			if (def.populationEffect >= IncidentPopulationEffect.Increase)
			{
				num = Find.Storyteller.intenderPopulation.PopulationIntent;
			}
			else if (def.populationEffect <= IncidentPopulationEffect.Decrease)
			{
				num = -Find.Storyteller.intenderPopulation.PopulationIntent;
			}
			if (num < 0.2f)
			{
				num = 0.2f;
			}
			return def.Worker.AdjustedChance * num;
		}

		private IncidentCategory DecideCategory()
		{
			int num = Find.TickManager.TicksGame - Find.StoryWatcher.storyState.LastThreatBigTick;
			if ((float)num > 60000f * this.Props.maxThreatBigIntervalDays)
			{
				return IncidentCategory.ThreatBig;
			}
			return this.Props.categoryWeights.RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
		}

		public override IncidentParms GenerateParms(IncidentCategory incCat)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, incCat);
			incidentParms.points *= Rand.Range(0.5f, 1.5f);
			return incidentParms;
		}
	}
}
