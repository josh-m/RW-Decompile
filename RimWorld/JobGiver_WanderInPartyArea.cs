using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_WanderInPartyArea : JobGiver_Wander
	{
		protected override IntVec3 GetExactWanderDest(Pawn pawn)
		{
			IntVec3 result;
			if (!PartyUtility.TryFindRandomCellInPartyArea(pawn, out result))
			{
				return IntVec3.Invalid;
			}
			return result;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			throw new NotImplementedException();
		}
	}
}
