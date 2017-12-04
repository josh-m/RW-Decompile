using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Ambush_ManhunterPack : IncidentWorker_Ambush
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			PawnKindDef pawnKindDef;
			return ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, -1, out pawnKindDef) && base.TryExecuteWorker(parms);
		}

		protected override List<Pawn> GeneratePawns(IncidentParms parms)
		{
			PawnKindDef animalKind;
			if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, parms.target.Tile, out animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, -1, out animalKind))
			{
				Log.Error("Could not find any valid animal kind for " + this.def + " incident.");
				return new List<Pawn>();
			}
			return ManhunterPackIncidentUtility.GenerateAnimals(animalKind, parms.target.Tile, parms.points);
		}

		protected override void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
		{
			for (int i = 0; i < generatedPawns.Count; i++)
			{
				generatedPawns[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null);
			}
		}

		protected override void SendAmbushLetter(Pawn anyPawn, IncidentParms parms)
		{
			base.SendStandardLetter(anyPawn, new string[]
			{
				anyPawn.GetKindLabelPlural(-1)
			});
		}
	}
}
