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
			float mtbNow = this.Props.mtbDays;
			if (this.Props.mtbDaysFactorByDaysPassedCurve != null)
			{
				mtbNow *= this.Props.mtbDaysFactorByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
			}
			IncidentDef selectedDef;
			if (Rand.MTBEventOccurs(mtbNow, 60000f, 1000f) && base.UsableIncidentsInCategory(this.Props.category, target).TryRandomElementByWeight((IncidentDef incDef) => this.$this.IncidentChanceFinal(incDef), out selectedDef))
			{
				yield return new FiringIncident(selectedDef, this, this.GenerateParms(selectedDef.category, target));
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + this.Props.category;
		}
	}
}
