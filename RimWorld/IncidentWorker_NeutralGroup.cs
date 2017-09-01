using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_NeutralGroup : IncidentWorker_PawnsArrive
	{
		protected virtual PawnGroupKindDef PawnGroupKindDef
		{
			get
			{
				return PawnGroupKindDefOf.Normal;
			}
		}

		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && !f.def.hidden && !f.HostileTo(Faction.OfPlayer);
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
			return (parms.spawnCenter.IsValid || RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, null)) && (parms.faction != null || base.CandidateFactions(map, false).TryRandomElement(out parms.faction) || base.CandidateFactions(map, true).TryRandomElement(out parms.faction));
		}

		protected virtual void ResolveParmsPoints(IncidentParms parms)
		{
			if (parms.points < 0f)
			{
				float value = Rand.Value;
				if (value < 0.4f)
				{
					parms.points = (float)Rand.Range(40, 140);
				}
				else if (value < 0.8f)
				{
					parms.points = (float)Rand.Range(140, 200);
				}
				else
				{
					parms.points = (float)Rand.Range(200, 500);
				}
			}
			IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
		}

		protected List<Pawn> SpawnPawns(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(this.PawnGroupKindDef, defaultPawnGroupMakerParms, false).ToList<Pawn>();
			foreach (Pawn current in list)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5, null);
				GenSpawn.Spawn(current, loc, map);
			}
			return list;
		}
	}
}
