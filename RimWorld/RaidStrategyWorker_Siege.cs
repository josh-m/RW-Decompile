using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_Siege : RaidStrategyWorker
	{
		private const float MinPointsForSiege = 350f;

		public override LordJob MakeLordJob(IncidentParms parms, Map map)
		{
			IntVec3 siegeSpot = RCellFinder.FindSiegePositionFrom(parms.spawnCenter, map);
			float num = parms.points * Rand.Range(0.2f, 0.3f);
			if (num < 60f)
			{
				num = 60f;
			}
			return new LordJob_Siege(parms.faction, siegeSpot, num);
		}

		public override float MinimumPoints(Faction fac)
		{
			return Mathf.Max(base.MinimumPoints(fac), 350f);
		}

		public override bool CanUseWith(IncidentParms parms)
		{
			return base.CanUseWith(parms) && parms.faction.def.canSiege;
		}
	}
}
