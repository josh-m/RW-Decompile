using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnGroupKindWorker_Normal : PawnGroupKindWorker
	{
		public override float MinPointsToGenerateAnything(PawnGroupMaker groupMaker)
		{
			return groupMaker.options.Min((PawnGenOption g) => g.Cost);
		}

		public override bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			return base.CanGenerateFrom(parms, groupMaker) && PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms).Any<PawnGenOption>();
		}

		protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
		{
			if (!this.CanGenerateFrom(parms, groupMaker))
			{
				if (errorOnZeroResults)
				{
					Log.Error(string.Concat(new object[]
					{
						"Cannot generate pawns for ",
						parms.faction,
						" with ",
						parms.points,
						". Defaulting to a single random cheap group."
					}));
				}
				return;
			}
			bool flag = parms.raidStrategy == null || parms.raidStrategy.pawnsCanBringFood;
			bool flag2 = false;
			foreach (PawnGenOption current in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms))
			{
				int tile = parms.tile;
				bool allowFood = flag;
				bool inhabitants = parms.inhabitants;
				PawnGenerationRequest request = new PawnGenerationRequest(current.kind, parms.faction, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, true, 1f, false, true, allowFood, inhabitants, false, null, null, null, null, null, null);
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				if (parms.forceOneIncap && !flag2)
				{
					pawn.health.forceIncap = true;
					pawn.mindState.canFleeIndividual = false;
					flag2 = true;
				}
				outPawns.Add(pawn);
			}
		}
	}
}
