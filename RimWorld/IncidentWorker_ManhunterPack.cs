using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ManhunterPack : IncidentWorker
	{
		private const int AnimalsStayDurationMin = 60000;

		private const int AnimalsStayDurationMax = 135000;

		private const float PointsFactor = 1.4f;

		public override bool TryExecute(IncidentParms parms)
		{
			PawnKindDef pawnKindDef;
			if (!(from k in this.def.pawnKinds
			where GenTemperature.SeasonAndOutdoorTemperatureAcceptableFor(k.race)
			select k).TryRandomElement(out pawnKindDef))
			{
				return false;
			}
			IntVec3 root;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out root))
			{
				return false;
			}
			int num = Mathf.Max(Mathf.RoundToInt(parms.points * 1.4f / pawnKindDef.combatPower), 1);
			Thing t = null;
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(root, 10);
				Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
				GenSpawn.Spawn(pawn, loc);
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false);
				pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 135000);
				t = pawn;
			}
			Find.LetterStack.ReceiveLetter(new Letter("LetterLabelManhunterPackArrived".Translate(), "ManhunterPackArrived".Translate(new object[]
			{
				pawnKindDef.label
			}), LetterType.BadUrgent, t), null);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
			return true;
		}
	}
}
