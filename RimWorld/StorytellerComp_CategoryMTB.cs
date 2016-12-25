using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_CategoryMTB : StorytellerComp
	{
		protected StorytellerCompProperties_CategoryMTB Props
		{
			get
			{
				return (StorytellerCompProperties_CategoryMTB)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents()
		{
			if (Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 1000f))
			{
				IncidentDef selectedDef = this.UsableIncidentsInCategory(this.Props.category).RandomElementByWeight((IncidentDef incDef) => this.<>f__this.IncidentChanceAdjustedForPopulation(incDef));
				yield return new FiringIncident(selectedDef, this, this.GenerateParms(selectedDef.category));
			}
		}
	}
}
