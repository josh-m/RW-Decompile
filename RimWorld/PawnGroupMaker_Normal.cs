using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnGroupMaker_Normal : PawnGroupMaker
	{
		public List<PawnGenOption> options = new List<PawnGenOption>();

		public override float MinPointsToGenerateAnything
		{
			get
			{
				return this.options.Min((PawnGenOption g) => g.Cost);
			}
		}

		public override bool CanGenerateFrom(IncidentParms parms)
		{
			return !parms.traderCaravan && base.CanGenerateFrom(parms) && PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, this.options, parms).Any<PawnGenOption>();
		}

		[DebuggerHidden]
		public override IEnumerable<Pawn> GenerateArrivingPawns(IncidentParms parms, bool errorOnZeroResults = true)
		{
			if (!this.CanGenerateFrom(parms))
			{
				if (errorOnZeroResults)
				{
					Log.Error(string.Concat(new object[]
					{
						"Cannot generate arriving pawns for ",
						parms.faction,
						" with ",
						parms.points,
						". Defaulting to a single random cheap group."
					}));
				}
			}
			else
			{
				bool allowFood = parms.raidStrategy != null && parms.raidStrategy.pawnsCanBringFood;
				bool forceIncapDone = false;
				foreach (PawnGenOption g in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, this.options, parms))
				{
					bool allowFood2 = allowFood;
					PawnGenerationRequest req = new PawnGenerationRequest(g.kind, parms.faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, true, 1f, false, true, allowFood2, null, null, null, null, null, null);
					Pawn p = PawnGenerator.GeneratePawn(req);
					if (parms.raidForceOneIncap && !forceIncapDone)
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
