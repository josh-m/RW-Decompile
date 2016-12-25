using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_Disease : StorytellerComp
	{
		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents()
		{
			if (DebugSettings.enableRandomDiseases)
			{
				float mtb = Find.Map.Biome.diseaseMtbDays;
				mtb *= Find.Storyteller.difficulty.diseaseIntervalFactor;
				if (Rand.MTBEventOccurs(mtb, 60000f, 1000f))
				{
					IncidentDef inc;
					if ((from d in DefDatabase<IncidentDef>.AllDefs
					where d.category == IncidentCategory.Disease
					select d).TryRandomElementByWeight((IncidentDef d) => Find.Map.Biome.CommonalityOfDisease(d), out inc))
					{
						yield return new FiringIncident(inc, this, this.GenerateParms(inc.category));
					}
				}
			}
		}
	}
}
