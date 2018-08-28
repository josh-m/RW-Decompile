using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_SingleMTB : StorytellerComp
	{
		private StorytellerCompProperties_SingleMTB Props
		{
			get
			{
				return (StorytellerCompProperties_SingleMTB)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (this.Props.incident.TargetAllowed(target))
			{
				if (Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 1000f))
				{
					IncidentParms parms = this.GenerateParms(this.Props.incident.category, target);
					if (this.Props.incident.Worker.CanFireNow(parms, false))
					{
						yield return new FiringIncident(this.Props.incident, this, parms);
					}
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + this.Props.incident;
		}
	}
}
