using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class IncidentParmsUtility
	{
		public static PawnGroupMakerParms GetDefaultPawnGroupMakerParms(IncidentParms parms)
		{
			return new PawnGroupMakerParms
			{
				map = (parms.target as Map),
				points = parms.points,
				faction = parms.faction,
				traderKind = parms.traderKind,
				generateFightersOnly = parms.generateFightersOnly,
				generateMeleeOnly = parms.generateMeleeOnly,
				raidStrategy = parms.raidStrategy,
				forceOneIncap = parms.raidForceOneIncap
			};
		}

		public static void AdjustPointsForGroupArrivalParams(IncidentParms parms)
		{
			if (parms.raidStrategy != null)
			{
				parms.points *= parms.raidStrategy.pointsFactor;
			}
			switch (parms.raidArrivalMode)
			{
			case PawnsArriveMode.EdgeWalkIn:
				parms.points *= 1f;
				break;
			case PawnsArriveMode.EdgeDrop:
				parms.points *= 1f;
				break;
			case PawnsArriveMode.CenterDrop:
				parms.points *= 0.45f;
				break;
			}
			if (parms.raidStrategy != null)
			{
				parms.points = Mathf.Max(parms.points, parms.raidStrategy.Worker.MinimumPoints(parms.faction) * 1.05f);
			}
		}
	}
}
