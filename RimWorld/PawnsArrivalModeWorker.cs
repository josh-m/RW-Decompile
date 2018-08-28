using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class PawnsArrivalModeWorker
	{
		public PawnsArrivalModeDef def;

		public virtual bool CanUseWith(IncidentParms parms)
		{
			return (parms.faction == null || this.def.minTechLevel == TechLevel.Undefined || parms.faction.def.techLevel >= this.def.minTechLevel) && (!parms.raidArrivalModeForQuickMilitaryAid || this.def.forQuickMilitaryAid) && (parms.raidStrategy == null || parms.raidStrategy.arriveModes.Contains(this.def));
		}

		public virtual float GetSelectionWeight(IncidentParms parms)
		{
			return this.def.selectionWeightCurve.Evaluate(parms.points);
		}

		public abstract void Arrive(List<Pawn> pawns, IncidentParms parms);

		public virtual void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			throw new NotSupportedException("Traveling transport pods arrived with mode " + this.def.defName);
		}

		public abstract bool TryResolveRaidSpawnCenter(IncidentParms parms);
	}
}
