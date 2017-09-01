using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class PawnGroupKindWorker
	{
		public PawnGroupKindDef def;

		public static List<Pawn> pawnsBeingGeneratedNow;

		public abstract float MinPointsToGenerateAnything(PawnGroupMaker groupMaker);

		public List<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults = true)
		{
			if (PawnGroupKindWorker.pawnsBeingGeneratedNow != null)
			{
				Log.Error("pawnsBeingGeneratedNow is not null. Nested calls are not allowed.");
			}
			List<Pawn> list = new List<Pawn>();
			PawnGroupKindWorker.pawnsBeingGeneratedNow = list;
			try
			{
				this.GeneratePawns(parms, groupMaker, list, errorOnZeroResults);
			}
			finally
			{
				PawnGroupKindWorker.pawnsBeingGeneratedNow = null;
			}
			return list;
		}

		protected abstract void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true);

		public virtual bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			return true;
		}
	}
}
