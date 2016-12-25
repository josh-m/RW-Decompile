using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
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
				List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < allFactionsListForReading.Count; i++)
				{
					if (!allFactionsListForReading[i].def.hidden && !allFactionsListForReading[i].IsPlayer)
					{
						if (allFactionsListForReading[i].def.CanEverBeNonHostile)
						{
							num2++;
						}
						if (!allFactionsListForReading[i].HostileTo(Faction.OfPlayer))
						{
							num++;
						}
					}
				}
				if (num == 0)
				{
					return -1f;
				}
				float num3 = (float)num / Mathf.Max((float)num2, 1f);
				return this.Props.baseMtb / num3;
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
				if (!Find.StoryWatcher.storyState.lastFireTicks.TryGetValue(IncidentDefOf.TraderCaravanArrival, out num))
				{
					num = (int)(this.props.minDaysPassed * 60000f);
				}
				if (Find.TickManager.TicksGame > num + 780000)
				{
					result = IncidentDefOf.TraderCaravanArrival;
					return true;
				}
			}
			return (from d in DefDatabase<IncidentDef>.AllDefs
			where d.category == IncidentCategory.AllyArrival && d.TargetAllowed(target) && d.Worker.CanFireNow(target)
			select d).TryRandomElementByWeight((IncidentDef d) => d.baseChance, out result);
		}
	}
}
