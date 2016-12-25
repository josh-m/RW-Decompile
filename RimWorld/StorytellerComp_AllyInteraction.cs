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

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents()
		{
			List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
			int nonHostileCount = 0;
			int canBeNonHostileCount = 0;
			for (int i = 0; i < factions.Count; i++)
			{
				if (!factions[i].def.hidden && !factions[i].IsPlayer)
				{
					if (factions[i].def.CanEverBeNonHostile)
					{
						canBeNonHostileCount++;
					}
					if (!factions[i].HostileTo(Faction.OfPlayer))
					{
						nonHostileCount++;
					}
				}
			}
			if (nonHostileCount != 0)
			{
				float freqFraction = (float)nonHostileCount / Mathf.Max((float)canBeNonHostileCount, 1f);
				float adjustedMtb = this.Props.baseMtb / freqFraction;
				IncidentDef incDef;
				if (Rand.MTBEventOccurs(adjustedMtb, 60000f, 1000f) && this.TryChooseIncident(out incDef))
				{
					yield return new FiringIncident(incDef, this, this.GenerateParms(incDef.category));
				}
			}
		}

		private bool TryChooseIncident(out IncidentDef result)
		{
			int num;
			if (Find.StoryWatcher.storyState.lastFireTicks.TryGetValue(IncidentDefOf.TraderCaravanArrival, out num) && Find.TickManager.TicksGame > num + 780000)
			{
				result = IncidentDefOf.TraderCaravanArrival;
				return true;
			}
			return (from d in DefDatabase<IncidentDef>.AllDefs
			where d.category == IncidentCategory.AllyArrival && d.Worker.CanFireNow()
			select d).TryRandomElementByWeight((IncidentDef d) => d.baseChance, out result);
		}
	}
}
