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
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 1000f))
			{
				List<IncidentCategory> triedCategories = new List<IncidentCategory>();
				IEnumerable<IncidentDef> options;
				while (true)
				{
					if (triedCategories.Count >= this.Props.categoryWeights.Count)
					{
						break;
					}
					IncidentCategory category = this.DecideCategory(target, triedCategories);
					triedCategories.Add(category);
					IncidentParms parms = this.GenerateParms(category, target);
					options = from d in DefDatabase<IncidentDef>.AllDefs
					where d.category == category && d.Worker.CanFireNow(target) && (!d.NeedsParms || d.minThreatPoints <= parms.points)
					select d;
					if (options.Any<IncidentDef>())
					{
						goto Block_2;
					}
				}
				return;
				Block_2:
				IncidentDef incDef;
				if (options.TryRandomElementByWeight(new Func<IncidentDef, float>(base.IncidentChanceFinal), out incDef))
				{
					yield return new FiringIncident(incDef, this, this.GenerateParms(incDef.category, target));
				}
			}
		}

		private IncidentCategory DecideCategory(IIncidentTarget target, List<IncidentCategory> skipCategories)
		{
			if (!skipCategories.Contains(IncidentCategory.ThreatBig))
			{
				int num = Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick;
				if ((float)num > 60000f * this.Props.maxThreatBigIntervalDays)
				{
					return IncidentCategory.ThreatBig;
				}
			}
			return (from cw in this.Props.categoryWeights
			where !skipCategories.Contains(cw.category)
			select cw).RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
		}

		public override IncidentParms GenerateParms(IncidentCategory incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, incCat, target);
			incidentParms.points *= Rand.Range(0.5f, 1.5f);
			return incidentParms;
		}
	}
}
