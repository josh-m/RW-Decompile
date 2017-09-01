using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Ambush_ManhunterPack : IncidentWorker_Ambush
	{
		public override bool TryExecute(IncidentParms parms)
		{
			PawnKindDef pawnKindDef;
			return ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, -1, out pawnKindDef) && base.TryExecute(parms);
		}

		protected override List<Pawn> GeneratePawns(IIncidentTarget target, float points, int tile)
		{
			PawnKindDef animalKind;
			if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(points, tile, out animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(points, -1, out animalKind))
			{
				Log.Error("Could not find any valid animal kind for " + this.def + " incident.");
				return new List<Pawn>();
			}
			return ManhunterPackIncidentUtility.GenerateAnimals(animalKind, tile, points);
		}

		protected override void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
		{
			for (int i = 0; i < generatedPawns.Count; i++)
			{
				generatedPawns[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null);
			}
		}

		protected override void SendAmbushLetter(Pawn anyPawn, Faction enemyFaction)
		{
			base.SendStandardLetter(anyPawn, new string[]
			{
				anyPawn.KindLabelPlural
			});
		}
	}
}
