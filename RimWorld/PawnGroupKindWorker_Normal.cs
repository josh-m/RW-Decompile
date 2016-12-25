using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		[DebuggerHidden]
		public override IEnumerable<Pawn> GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, bool errorOnZeroResults = true)
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
			}
			else
			{
				bool allowFood = parms.raidStrategy == null || parms.raidStrategy.pawnsCanBringFood;
				bool forceIncapDone = false;
				foreach (PawnGenOption g in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms))
				{
					Map map = parms.map;
					bool allowFood2 = allowFood;
					PawnGenerationRequest request = new PawnGenerationRequest(g.kind, parms.faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, true, 1f, false, true, allowFood2, null, null, null, null, null, null);
					Pawn p = PawnGenerator.GeneratePawn(request);
					if (parms.forceOneIncap && !forceIncapDone)
					{
						p.health.forceIncap = true;
						p.mindState.canFleeIndividual = false;
						forceIncapDone = true;
					}
					this.PostGenerate(p);
					yield return p;
				}
				this.FinishedGeneratingPawns();
			}
		}
	}
}
