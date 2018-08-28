using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_Disease : StorytellerComp
	{
		protected StorytellerCompProperties_Disease Props
		{
			get
			{
				return (StorytellerCompProperties_Disease)this.props;
			}
		}

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
					if (Rand.MTBEventOccurs(mtb, 60000f, 1000f) && base.UsableIncidentsInCategory(this.Props.category, target).TryRandomElementByWeight((IncidentDef d) => biome.CommonalityOfDisease(d), out inc))
					{
						yield return new FiringIncident(inc, this, this.GenerateParms(inc.category, target));
					}
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + this.Props.category;
		}
	}
}
