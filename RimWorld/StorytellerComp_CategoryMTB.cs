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
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			IncidentDef selectedDef;
			if (Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 1000f) && this.UsableIncidentsInCategory(this.Props.category, target).TryRandomElementByWeight((IncidentDef incDef) => this.$this.IncidentChanceFinal(incDef), out selectedDef))
			{
				yield return new FiringIncident(selectedDef, this, this.GenerateParms(selectedDef.category, target));
			}
		}
	}
}
