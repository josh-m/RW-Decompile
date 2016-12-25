using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttackSmart : RaidStrategyWorker
	{
		public override LordJob MakeLordJob(IncidentParms parms, Map map)
		{
			return new LordJob_AssaultColony(parms.faction, true, true, false, true, true);
		}

		public override bool CanUseWith(IncidentParms parms)
		{
			return base.CanUseWith(parms) && parms.faction.def.canUseAvoidGrid;
		}
	}
}
