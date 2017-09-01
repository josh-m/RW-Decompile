using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_Ambush_EnemyFaction : IncidentWorker_Ambush
	{
		private Faction randomFaction;

		protected override List<Pawn> GeneratePawns(IIncidentTarget target, float points, int tile)
		{
			this.randomFaction = Find.FactionManager.RandomEnemyFaction(false, false, true);
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = tile;
			pawnGroupMakerParms.generateFightersOnly = true;
			pawnGroupMakerParms.faction = this.randomFaction;
			pawnGroupMakerParms.points = points;
			return PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, pawnGroupMakerParms, true).ToList<Pawn>();
		}

		protected override LordJob CreateLordJob(List<Pawn> generatedPawns, out Faction faction)
		{
			LordJob_AssaultColony result = new LordJob_AssaultColony(this.randomFaction, true, false, false, false, true);
			faction = this.randomFaction;
			this.randomFaction = null;
			return result;
		}

		protected override void SendAmbushLetter(Pawn anyPawn, Faction enemyFaction)
		{
			base.SendStandardLetter(anyPawn, new string[]
			{
				enemyFaction.def.pawnsPlural,
				enemyFaction.Name
			});
		}
	}
}
