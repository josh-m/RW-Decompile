using System;
using UnityEngine;

namespace RimWorld
{
	public static class IncidentParmsUtility
	{
		public static PawnGroupMakerParms GetDefaultPawnGroupMakerParms(IncidentParms parms, bool ensureCanGenerateAtLeastOnePawn = false)
		{
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = parms.target.Tile;
			pawnGroupMakerParms.points = parms.points;
			pawnGroupMakerParms.faction = parms.faction;
			pawnGroupMakerParms.traderKind = parms.traderKind;
			pawnGroupMakerParms.generateFightersOnly = parms.generateFightersOnly;
			pawnGroupMakerParms.raidStrategy = parms.raidStrategy;
			pawnGroupMakerParms.forceOneIncap = parms.raidForceOneIncap;
			if (ensureCanGenerateAtLeastOnePawn && parms.faction != null)
			{
				pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, parms.faction.def.MinPointsToGenerateNormalPawnGroup());
			}
			return pawnGroupMakerParms;
		}

		public static void AdjustPointsForGroupArrivalParams(IncidentParms parms)
		{
			if (parms.raidStrategy != null)
			{
				parms.points *= parms.raidStrategy.pointsFactor;
			}
			PawnsArriveMode raidArrivalMode = parms.raidArrivalMode;
			if (raidArrivalMode != PawnsArriveMode.EdgeWalkIn)
			{
				if (raidArrivalMode != PawnsArriveMode.EdgeDrop)
				{
					if (raidArrivalMode == PawnsArriveMode.CenterDrop)
					{
						parms.points *= 0.45f;
					}
				}
				else
				{
					parms.points *= 1f;
				}
			}
			else
			{
				parms.points *= 1f;
			}
			if (parms.raidStrategy != null)
			{
				parms.points = Mathf.Max(parms.points, parms.raidStrategy.Worker.MinimumPoints(parms.faction) * 1.05f);
			}
		}
	}
}
