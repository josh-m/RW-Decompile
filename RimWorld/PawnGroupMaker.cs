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

		public float maxTotalPoints = 9999999f;

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
			return this.kindDef.Worker.GeneratePawns(parms, this, errorOnZeroResults);
		}

		public IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms)
		{
			return this.kindDef.Worker.GeneratePawnKindsExample(parms, this);
		}

		public bool CanGenerateFrom(PawnGroupMakerParms parms)
		{
			return parms.points <= this.maxTotalPoints && (this.disallowedStrategies == null || !this.disallowedStrategies.Contains(parms.raidStrategy)) && this.kindDef.Worker.CanGenerateFrom(parms, this);
		}
	}
}
