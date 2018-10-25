using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_NeutralGroup : IncidentWorker_PawnsArrive
	{
		protected virtual PawnGroupKindDef PawnGroupKindDef
		{
			get
			{
				return PawnGroupKindDefOf.Peaceful;
			}
		}

		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) && !NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f);
		}

		protected bool TryResolveParms(IncidentParms parms)
		{
			if (!this.TryResolveParmsGeneral(parms))
			{
				return false;
			}
			this.ResolveParmsPoints(parms);
			return true;
		}

		protected virtual bool TryResolveParmsGeneral(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			return (parms.spawnCenter.IsValid || RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, false, null)) && (parms.faction != null || base.CandidateFactions(map, false).TryRandomElement(out parms.faction) || base.CandidateFactions(map, true).TryRandomElement(out parms.faction));
		}

		protected abstract void ResolveParmsPoints(IncidentParms parms);

		protected List<Pawn> SpawnPawns(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(this.PawnGroupKindDef, parms, true);
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, false).ToList<Pawn>();
			foreach (Pawn current in list)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5, null);
				GenSpawn.Spawn(current, loc, map, WipeMode.Vanish);
			}
			return list;
		}
	}
}
