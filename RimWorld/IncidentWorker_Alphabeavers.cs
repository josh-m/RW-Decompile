using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_Alphabeavers : IncidentWorker
	{
		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
			IntVec3 intVec;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map))
			{
				return false;
			}
			int num = Rand.RangeInclusive(6, 12);
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
				Pawn newThing = PawnGenerator.GeneratePawn(alphabeaver, null);
				GenSpawn.Spawn(newThing, loc, map);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterType.BadNonUrgent, new TargetInfo(intVec, map, false), null);
			return true;
		}
	}
}
