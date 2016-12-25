using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ThrumboPasses : IncidentWorker
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return !map.mapConditionManager.ConditionIsActive(MapConditionDefOf.ToxicFallout);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map))
			{
				return false;
			}
			PawnKindDef thrumbo = PawnKindDefOf.Thrumbo;
			float points = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentCategory.ThreatBig, map).points;
			int num = GenMath.RoundRandom(points / thrumbo.combatPower);
			int max = Rand.RangeInclusive(2, 4);
			num = Mathf.Clamp(num, 1, max);
			int num2 = Rand.RangeInclusive(90000, 150000);
			IntVec3 invalid = IntVec3.Invalid;
			if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, map, 10f, out invalid))
			{
				invalid = IntVec3.Invalid;
			}
			Pawn pawn = null;
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
				pawn = PawnGenerator.GeneratePawn(thrumbo, null);
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
				pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num2;
				if (invalid.IsValid)
				{
					pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, map, 10);
				}
			}
			Find.LetterStack.ReceiveLetter("LetterLabelThrumboPasses".Translate(new object[]
			{
				thrumbo.label
			}).CapitalizeFirst(), "LetterThrumboPasses".Translate(new object[]
			{
				thrumbo.label
			}), LetterType.Good, pawn, null);
			return true;
		}
	}
}
