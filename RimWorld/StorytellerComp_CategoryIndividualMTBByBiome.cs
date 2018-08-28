using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
								if (this.Props.applyCaravanVisibility)
								{
									Caravan caravan = target as Caravan;
									if (caravan != null)
									{
										mtb /= caravan.Visibility;
									}
									else
									{
										Map map = target as Map;
										if (map != null && map.Parent.def.isTempIncidentMapOwner)
										{
											IEnumerable<Pawn> pawns = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Concat(map.mapPawns.PrisonersOfColonySpawned);
											mtb /= CaravanVisibilityCalculator.Visibility(pawns, false, null);
										}
									}
								}
								if (Rand.MTBEventOccurs(mtb, 60000f, 1000f))
								{
									IncidentParms parms = this.GenerateParms(inc.category, target);
									if (inc.Worker.CanFireNow(parms, false))
									{
										yield return new FiringIncident(inc, this, parms);
									}
								}
							}
						}
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
