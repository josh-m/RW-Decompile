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
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.DamageUntilDowned(pawn);
			string label = "LetterLabelRefugeePodCrash".Translate();
			string text = "RefugeePodCrash".Translate();
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.BadNonUrgent, new TargetInfo(intVec, map, false), null);
			DropPodUtility.MakeDropPodAt(intVec, map, new ActiveDropPodInfo
			{
				SingleContainedThing = ((!pawn.Dead) ? pawn : pawn.Corpse),
				openDelay = 180,
				leaveSlag = true
			});
			return true;
		}
	}
}
