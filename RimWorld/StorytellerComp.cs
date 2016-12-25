using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		protected virtual float IncidentChanceAdjustedForPopulation(IncidentDef def)
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
			num = Mathf.Max(num, 0.05f);
			return Mathf.Max(0f, def.Worker.AdjustedChance * num);
		}

		public virtual void DebugLogIncidentChances(IncidentCategory cat)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetType() + ":");
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"PopulationIntent: ",
				Find.Storyteller.intenderPopulation.PopulationIntent,
				", AdjustedPopulation: ",
				Find.Storyteller.intenderPopulation.AdjustedPopulation
			}));
			foreach (IncidentDef current in from d in this.UsableIncidentsInCategory(cat, Find.VisibleMap)
			orderby this.IncidentChanceAdjustedForPopulation(d) descending
			select d)
			{
				stringBuilder.AppendLine(current.defName.PadRight(25) + this.IncidentChanceAdjustedForPopulation(current).ToString());
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
