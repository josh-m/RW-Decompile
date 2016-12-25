using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnGroupMaker
	{
		public PawnGroupKindDef kindDef;

		public float commonality = 100f;

		public List<RaidStrategyDef> disallowedStrategies;

		public List<PawnGenOption> options = new List<PawnGenOption>();

		public List<PawnGenOption> traders = new List<PawnGenOption>();

		public List<PawnGenOption> carriers = new List<PawnGenOption>();

		public List<PawnGenOption> guards = new List<PawnGenOption>();

		public float MinPointsToGenerateAnything
		{
			get
			{
				return this.kindDef.Worker.MinPointsToGenerateAnything(this);
			}
		}

		public IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, bool errorOnZeroResults = true)
		{
			IEnumerable<Pawn> result;
			try
			{
				result = this.kindDef.Worker.GeneratePawns(parms, this, errorOnZeroResults);
			}
			finally
			{
				PawnGroupMakerUtility.ClearPawnsBeingGeneratedNow();
			}
			return result;
		}

		public bool CanGenerateFrom(PawnGroupMakerParms parms)
		{
			return (this.disallowedStrategies == null || !this.disallowedStrategies.Contains(parms.raidStrategy)) && this.kindDef.Worker.CanGenerateFrom(parms, this);
		}
	}
}
