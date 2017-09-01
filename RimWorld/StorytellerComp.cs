using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class StorytellerComp
	{
		public StorytellerCompProperties props;

		public abstract IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target);

		public virtual IncidentParms GenerateParms(IncidentCategory incCat, IIncidentTarget target)
		{
			return StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, incCat, target);
		}

		protected virtual IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategory cat, IIncidentTarget target)
		{
			return from x in DefDatabase<IncidentDef>.AllDefsListForReading
			where x.category == cat && x.Worker.CanFireNow(target)
			select x;
		}

		protected float IncidentChanceFactor_CurrentPopulation(IncidentDef def)
		{
			if (def.chanceFactorByPopulationCurve == null)
			{
				return 1f;
			}
			int num = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists.Count<Pawn>();
			return def.chanceFactorByPopulationCurve.Evaluate((float)num);
		}

		protected float IncidentChanceFactor_PopulationIntent(IncidentDef def)
		{
			IncidentPopulationEffect populationEffect = def.populationEffect;
			if (populationEffect == IncidentPopulationEffect.None)
			{
				return 1f;
			}
			if (populationEffect != IncidentPopulationEffect.Increase)
			{
				throw new NotImplementedException();
			}
			return Mathf.Max(Find.Storyteller.intenderPopulation.PopulationIntent, this.props.minIncChancePopulationIntentFactor);
		}

		protected float IncidentChanceFinal(IncidentDef def)
		{
			float num = def.Worker.AdjustedChance;
			num *= this.IncidentChanceFactor_CurrentPopulation(def);
			num *= this.IncidentChanceFactor_PopulationIntent(def);
			return Mathf.Max(0f, num);
		}

		public virtual void DebugTablesIncidentChances(IncidentCategory cat)
		{
			IEnumerable<IncidentDef> arg_192_0 = from d in DefDatabase<IncidentDef>.AllDefs
			where d.category == cat
			orderby this.IncidentChanceFinal(d) descending
			select d;
			TableDataGetter<IncidentDef>[] expr_42 = new TableDataGetter<IncidentDef>[10];
			expr_42[0] = new TableDataGetter<IncidentDef>("defName", (IncidentDef d) => d.defName);
			expr_42[1] = new TableDataGetter<IncidentDef>("baseChance", (IncidentDef d) => d.baseChance.ToString());
			expr_42[2] = new TableDataGetter<IncidentDef>("AdjustedChance", (IncidentDef d) => d.Worker.AdjustedChance.ToString());
			expr_42[3] = new TableDataGetter<IncidentDef>("Factor-PopCurrent", (IncidentDef d) => this.IncidentChanceFactor_CurrentPopulation(d).ToString());
			expr_42[4] = new TableDataGetter<IncidentDef>("Factor-PopIntent", (IncidentDef d) => this.IncidentChanceFactor_PopulationIntent(d).ToString());
			expr_42[5] = new TableDataGetter<IncidentDef>("final chance", (IncidentDef d) => this.IncidentChanceFinal(d).ToString());
			expr_42[6] = new TableDataGetter<IncidentDef>("vismap-usable", (IncidentDef d) => (Find.VisibleMap != null) ? ((!this.UsableIncidentsInCategory(cat, Find.VisibleMap).Contains(d)) ? string.Empty : "V") : "-");
			expr_42[7] = new TableDataGetter<IncidentDef>("world-usable", (IncidentDef d) => (!this.UsableIncidentsInCategory(cat, Find.World).Contains(d)) ? string.Empty : "W");
			expr_42[8] = new TableDataGetter<IncidentDef>("pop-current", (IncidentDef d) => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists.Count<Pawn>().ToString());
			expr_42[9] = new TableDataGetter<IncidentDef>("pop-intent", (IncidentDef d) => Find.Storyteller.intenderPopulation.PopulationIntent.ToString("F3"));
			DebugTables.MakeTablesDialog<IncidentDef>(arg_192_0, expr_42);
		}
	}
}
