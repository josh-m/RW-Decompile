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
				bool targetIsRaidBeacon = target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon);
				List<IncidentCategoryDef> triedCategories = new List<IncidentCategoryDef>();
				IncidentDef incDef;
				while (true)
				{
					IncidentCategoryDef category = this.ChooseRandomCategory(target, triedCategories);
					IncidentParms parms = this.GenerateParms(category, target);
					IEnumerable<IncidentDef> options = from d in base.UsableIncidentsInCategory(category, target)
					where !d.NeedsParmsPoints || parms.points >= d.minThreatPoints
					select d;
					if (options.TryRandomElementByWeight(new Func<IncidentDef, float>(base.IncidentChanceFinal), out incDef))
					{
						break;
					}
					triedCategories.Add(category);
					if (triedCategories.Count >= this.Props.categoryWeights.Count)
					{
						goto Block_5;
					}
				}
				if (!this.Props.skipThreatBigIfRaidBeacon || !targetIsRaidBeacon || incDef.category != IncidentCategoryDefOf.ThreatBig)
				{
					yield return new FiringIncident(incDef, this, <MakeIntervalIncidents>c__AnonStorey.parms);
				}
				Block_5:;
			}
		}

		private IncidentCategoryDef ChooseRandomCategory(IIncidentTarget target, List<IncidentCategoryDef> skipCategories)
		{
			if (!skipCategories.Contains(IncidentCategoryDefOf.ThreatBig))
			{
				int num = Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick;
				if (target.StoryState.LastThreatBigTick >= 0 && (float)num > 60000f * this.Props.maxThreatBigIntervalDays)
				{
					return IncidentCategoryDefOf.ThreatBig;
				}
			}
			return (from cw in this.Props.categoryWeights
			where !skipCategories.Contains(cw.category)
			select cw).RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
		}

		public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat, target);
			incidentParms.points *= this.Props.randomPointsFactorRange.RandomInRange;
			return incidentParms;
		}
	}
}
