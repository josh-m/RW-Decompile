using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_StageThenAttack : RaidStrategyWorker
	{
		public override LordJob MakeLordJob(ref IncidentParms parms)
		{
			IntVec3 stageLoc = RCellFinder.FindSiegePositionFrom(parms.spawnCenter);
			return new LordJob_StageThenAttack(parms.faction, stageLoc);
		}

		public override bool CanUseWith(IncidentParms parms)
		{
			return base.CanUseWith(parms) && parms.faction.def.canStageAttacks;
		}
	}
}
