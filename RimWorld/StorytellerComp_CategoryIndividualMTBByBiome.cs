using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_CategoryIndividualMTBByBiome : StorytellerComp
	{
		protected StorytellerCompProperties_CategoryIndividualMTBByBiome Props
		{
			get
			{
				return (StorytellerCompProperties_CategoryIndividualMTBByBiome)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (!(target is World))
			{
				List<IncidentDef> allIncidents = DefDatabase<IncidentDef>.AllDefsListForReading;
				for (int i = 0; i < allIncidents.Count; i++)
				{
					IncidentDef inc = allIncidents[i];
					if (inc.category == this.Props.category)
					{
						BiomeDef biome = Find.WorldGrid[target.Tile].biome;
						if (inc.mtbDaysByBiome != null)
						{
							MTBByBiome entry = inc.mtbDaysByBiome.Find((MTBByBiome x) => x.biome == this.<biome>__3);
							if (entry != null)
							{
								if (Rand.MTBEventOccurs(entry.mtbDays, 60000f, 1000f) && inc.Worker.CanFireNow(target))
								{
									yield return new FiringIncident(inc, this, this.GenerateParms(inc.category, target));
								}
							}
						}
					}
				}
			}
		}
	}
}
