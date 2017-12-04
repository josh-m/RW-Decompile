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
							MTBByBiome entry = inc.mtbDaysByBiome.Find((MTBByBiome x) => x.biome == biome);
							if (entry != null)
							{
								float mtb = entry.mtbDays;
								if (this.Props.applyCaravanStealthFactor)
								{
									Caravan caravan = target as Caravan;
									if (caravan != null)
									{
										mtb *= CaravanIncidentUtility.CalculateCaravanStealthFactor(caravan.PawnsListForReading.Count);
									}
									else
									{
										Map map = target as Map;
										if (map != null && map.info.parent.def.isTempIncidentMapOwner)
										{
											int pawnCount = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Count + map.mapPawns.PrisonersOfColonySpawnedCount;
											mtb *= CaravanIncidentUtility.CalculateCaravanStealthFactor(pawnCount);
										}
									}
								}
								if (Rand.MTBEventOccurs(mtb, 60000f, 1000f) && inc.Worker.CanFireNow(target))
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
