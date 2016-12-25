using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class PawnGroupMaker
	{
		public float commonality = 100f;

		private List<RaidStrategyDef> disallowedStrategies;

		[Unsaved]
		private static List<Pawn> makingPawns = new List<Pawn>();

		public static List<Pawn> MakingPawns
		{
			get
			{
				return PawnGroupMaker.makingPawns;
			}
		}

		public abstract float MinPointsToGenerateAnything
		{
			get;
		}

		public abstract IEnumerable<Pawn> GenerateArrivingPawns(IncidentParms parms, bool errorOnZeroResults = true);

		protected virtual void PostGenerate(Pawn pawn)
		{
			PawnGroupMaker.makingPawns.Add(pawn);
		}

		protected virtual void FinishedGeneratingPawns()
		{
			PawnGroupMaker.makingPawns.Clear();
		}

		public virtual bool CanGenerateFrom(IncidentParms parms)
		{
			return this.disallowedStrategies == null || !this.disallowedStrategies.Contains(parms.raidStrategy);
		}
	}
}
