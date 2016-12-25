using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_GreatGrandparent : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			return me != other && PawnRelationDefOf.GreatGrandchild.Worker.InRelation(other, me);
		}
	}
}
