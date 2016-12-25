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
			Map map = (Map)parms.target;
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, null, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.GiveInjuriesToForceDowned(pawn);
			string label = "LetterLabelRefugeePodCrash".Translate();
			string text = "RefugeePodCrash".Translate();
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, new TargetInfo(intVec, map, false), null);
			DropPodUtility.MakeDropPodAt(intVec, map, new ActiveDropPodInfo
			{
				SingleContainedThing = pawn,
				openDelay = 180,
				leaveSlag = true
			});
			return true;
		}
	}
}
