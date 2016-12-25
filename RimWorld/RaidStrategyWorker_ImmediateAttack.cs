using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttack : RaidStrategyWorker
	{
		public override LordJob MakeLordJob(ref IncidentParms parms)
		{
			if (parms.faction.HostileTo(Faction.OfPlayer))
			{
				return new LordJob_AssaultColony(parms.faction, true, true, false, false, true);
			}
			IntVec3 fallbackLocation;
			RCellFinder.TryFindRandomSpotJustOutsideColony(parms.spawnCenter, out fallbackLocation);
			return new LordJob_AssistColony(parms.faction, fallbackLocation);
		}
	}
}
