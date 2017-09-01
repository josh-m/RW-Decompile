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
				while (triedCategories.Count < this.Props.categoryWeights.Count)
				{
					IncidentCategory category = this.DecideCategory(target, triedCategories);
					triedCategories.Add(category);
					IncidentParms parms = this.GenerateParms(category, target);
					IEnumerable<IncidentDef> options = from d in DefDatabase<IncidentDef>.AllDefs
					where d.category == this.<category>__1 && d.Worker.CanFireNow(this.target) && (!d.NeedsParms || d.minThreatPoints <= this.<parms>__2.points)
					select d;
					if (options.Any<IncidentDef>())
					{
						IncidentDef incDef;
						if (options.TryRandomElementByWeight(new Func<IncidentDef, float>(base.IncidentChanceFinal), out incDef))
						{
							yield return new FiringIncident(incDef, this, this.GenerateParms(incDef.category, target));
							break;
						}
						break;
					}
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
