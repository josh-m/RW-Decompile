using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Grandparent : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			return me != other && PawnRelationDefOf.Grandchild.Worker.InRelation(other, me);
		}
	}
}
