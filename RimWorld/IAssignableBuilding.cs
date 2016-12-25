using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IAssignableBuilding
	{
		IEnumerable<Pawn> AssigningCandidates
		{
			get;
		}

		IEnumerable<Pawn> AssignedPawns
		{
			get;
		}

		int MaxAssignedPawnsCount
		{
			get;
		}

		void TryAssignPawn(Pawn pawn);

		void TryUnassignPawn(Pawn pawn);
	}
}
