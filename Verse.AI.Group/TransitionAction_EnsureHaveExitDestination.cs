using RimWorld;
using System;

namespace Verse.AI.Group
{
	public class TransitionAction_EnsureHaveExitDestination : TransitionAction
	{
		public override void DoAction(Transition trans)
		{
			LordToil_Travel lordToil_Travel = (LordToil_Travel)trans.target;
			if (lordToil_Travel.HasDestination())
			{
				return;
			}
			Pawn searcher = lordToil_Travel.lord.ownedPawns.RandomElement<Pawn>();
			IntVec3 destination;
			if (!CellFinder.TryFindRandomPawnExitCell(searcher, out destination))
			{
				RCellFinder.TryFindRandomPawnEntryCell(out destination);
			}
			lordToil_Travel.SetDestination(destination);
		}
	}
}
