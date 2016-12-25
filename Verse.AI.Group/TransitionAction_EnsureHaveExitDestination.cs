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
			Pawn pawn = lordToil_Travel.lord.ownedPawns.RandomElement<Pawn>();
			IntVec3 destination;
			if (!CellFinder.TryFindRandomPawnExitCell(pawn, out destination))
			{
				RCellFinder.TryFindRandomPawnEntryCell(out destination, pawn.Map);
			}
			lordToil_Travel.SetDestination(destination);
		}
	}
}
