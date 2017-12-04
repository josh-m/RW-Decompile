using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_Ambush_EnemyFaction : IncidentWorker_Ambush
	{
		protected override List<Pawn> GeneratePawns(IncidentParms parms)
		{
			if (!PawnGroupMakerUtility.TryGetRandomFactionForNormalPawnGroup(parms.points, out parms.faction, null, false, false, false, true))
			{
				return new List<Pawn>();
			}
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms, false);
			defaultPawnGroupMakerParms.generateFightersOnly = true;
			return PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
		}

		protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
		{
			return new LordJob_AssaultColony(parms.faction, true, false, false, false, true);
		}

		protected override void SendAmbushLetter(Pawn anyPawn, IncidentParms parms)
		{
			base.SendStandardLetter(anyPawn, new string[]
			{
				parms.faction.def.pawnsPlural,
				parms.faction.Name
			});
		}
	}
}
