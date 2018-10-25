using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_Alphabeavers : IncidentWorker
	{
		private static readonly FloatRange CountPerColonistRange = new FloatRange(1f, 1.5f);

		private const int MinCount = 1;

		private const int MaxCount = 10;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, CellFinder.EdgeRoadChance_Animal, false, null);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
			IntVec3 intVec;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, CellFinder.EdgeRoadChance_Animal, false, null))
			{
				return false;
			}
			int freeColonistsCount = map.mapPawns.FreeColonistsCount;
			float randomInRange = IncidentWorker_Alphabeavers.CountPerColonistRange.RandomInRange;
			float f = (float)freeColonistsCount * randomInRange;
			int num = Mathf.Clamp(GenMath.RoundRandom(f), 1, 10);
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
				Pawn newThing = PawnGenerator.GeneratePawn(alphabeaver, null);
				Pawn pawn = (Pawn)GenSpawn.Spawn(newThing, loc, map, WipeMode.Vanish);
				pawn.needs.food.CurLevelPercentage = 1f;
			}
			Find.LetterStack.ReceiveLetter("LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterDefOf.ThreatSmall, new TargetInfo(intVec, map, false), null, null);
			return true;
		}
	}
}
