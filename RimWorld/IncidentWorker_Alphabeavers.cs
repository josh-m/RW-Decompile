using System;
using Verse;

namespace RimWorld
{
	internal class IncidentWorker_Alphabeavers : IncidentWorker
	{
		public override bool TryExecute(IncidentParms parms)
		{
			PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
			IntVec3 intVec;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec))
			{
				return false;
			}
			int num = Rand.RangeInclusive(6, 12);
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, 10);
				Pawn newThing = PawnGenerator.GeneratePawn(alphabeaver, null);
				GenSpawn.Spawn(newThing, loc);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterType.BadNonUrgent, intVec, null);
			return true;
		}
	}
}
