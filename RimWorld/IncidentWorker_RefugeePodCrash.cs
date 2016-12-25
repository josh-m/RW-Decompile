using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RefugeePodCrash : IncidentWorker
	{
		private const float FogClearRadius = 4.5f;

		private const float RelationWithColonistWeight = 20f;

		public override bool TryExecute(IncidentParms parms)
		{
			IntVec3 intVec = DropCellFinder.RandomDropSpot();
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.GiveInjuriesToForceDowned(pawn);
			string label = "LetterLabelRefugeePodCrash".Translate();
			string text = "RefugeePodCrash".Translate();
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, intVec, null);
			DropPodUtility.MakeDropPodAt(intVec, new DropPodInfo
			{
				SingleContainedThing = pawn,
				openDelay = 180,
				leaveSlag = true
			});
			return true;
		}
	}
}
