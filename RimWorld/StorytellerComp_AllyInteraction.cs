using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_AllyInteraction : StorytellerComp
	{
		private const int ForceChooseTraderAfterTicks = 780000;

		private StorytellerCompProperties_AllyInteraction Props
		{
			get
			{
				return (StorytellerCompProperties_AllyInteraction)this.props;
			}
		}

		private float IncidentMTBDays
		{
			get
			{
				return this.Props.baseMtb * StorytellerUtility.AllyIncidentMTBMultiplier();
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			float mtb = this.IncidentMTBDays;
			if (mtb >= 0f)
			{
				IncidentDef incDef;
				if (Rand.MTBEventOccurs(mtb, 60000f, 1000f) && this.TryChooseIncident(target, out incDef))
				{
					yield return new FiringIncident(incDef, this, this.GenerateParms(incDef.category, target));
				}
			}
		}

		private bool TryChooseIncident(IIncidentTarget target, out IncidentDef result)
		{
			if (IncidentDefOf.TraderCaravanArrival.TargetAllowed(target))
			{
				int num = 0;
				if (!target.StoryState.lastFireTicks.TryGetValue(IncidentDefOf.TraderCaravanArrival, out num))
				{
					num = (int)(this.props.minDaysPassed * 60000f);
				}
				if (Find.TickManager.TicksGame > num + 780000)
				{
					result = IncidentDefOf.TraderCaravanArrival;
					return true;
				}
			}
			return this.UsableIncidentsInCategory(IncidentCategory.AllyArrival, target).TryRandomElementByWeight((IncidentDef d) => d.baseChance, out result);
		}
	}
}
