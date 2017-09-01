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
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (DebugSettings.enableRandomDiseases)
			{
				if (target.Tile != -1)
				{
					BiomeDef biome = Find.WorldGrid[target.Tile].biome;
					float mtb = biome.diseaseMtbDays;
					mtb *= Find.Storyteller.difficulty.diseaseIntervalFactor;
					IncidentDef inc;
					if (Rand.MTBEventOccurs(mtb, 60000f, 1000f) && (from d in DefDatabase<IncidentDef>.AllDefs
					where d.TargetAllowed(this.target) && d.category == IncidentCategory.Disease
					select d).TryRandomElementByWeight((IncidentDef d) => this.<biome>__0.CommonalityOfDisease(d), out inc))
					{
						yield return new FiringIncident(inc, this, this.GenerateParms(inc.category, target));
					}
				}
			}
		}
	}
}
