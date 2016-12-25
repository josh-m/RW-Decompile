using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ManhunterPack : IncidentWorker
	{
		private const float PointsFactor = 1.4f;

		private const int AnimalsStayDurationMin = 60000;

		private const int AnimalsStayDurationMax = 135000;

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef pawnKindDef;
			if (!ManhunterPackIncidentUtility.TryFindRandomAnimalKind(this.def.pawnKinds, map, out pawnKindDef))
			{
				return false;
			}
			IntVec3 root;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out root, map))
			{
				return false;
			}
			List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals(pawnKindDef, map, parms.points * 1.4f);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(root, map, 10);
				GenSpawn.Spawn(pawn, loc, map);
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null);
				pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 135000);
			}
			Find.LetterStack.ReceiveLetter(new Letter("LetterLabelManhunterPackArrived".Translate(), "ManhunterPackArrived".Translate(new object[]
			{
				pawnKindDef.label
			}), LetterType.BadUrgent, list[0]), null);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
			return true;
		}
	}
}
