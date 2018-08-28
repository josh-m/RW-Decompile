using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeWorker_RandomDrop : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			for (int i = 0; i < pawns.Count; i++)
			{
				IntVec3 dropCenter = DropCellFinder.RandomDropSpot(map);
				DropPodUtility.DropThingsNear(dropCenter, map, Gen.YieldSingle<Thing>(pawns[i]), parms.podOpenDelay, false, true, true);
			}
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			parms.podOpenDelay = 520;
			parms.spawnRotation = Rot4.Random;
			return true;
		}
	}
}
