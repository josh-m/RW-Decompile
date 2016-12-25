using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class PawnGroupKindWorker
	{
		public PawnGroupKindDef def;

		public abstract float MinPointsToGenerateAnything(PawnGroupMaker groupMaker);

		public abstract IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults = true);

		protected virtual void PostGenerate(Pawn pawn)
		{
			PawnGroupMakerUtility.AddToPawnsBeingGeneratedNow(pawn);
		}

		protected virtual void FinishedGeneratingPawns()
		{
			PawnGroupMakerUtility.ClearPawnsBeingGeneratedNow();
		}

		public virtual bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			return true;
		}
	}
}
