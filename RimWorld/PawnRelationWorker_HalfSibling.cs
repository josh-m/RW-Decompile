using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_HalfSibling : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			return me != other && !PawnRelationDefOf.Sibling.Worker.InRelation(me, other) && ((me.GetMother() != null && me.GetMother() == other.GetMother()) || (me.GetFather() != null && me.GetFather() == other.GetFather()));
		}
	}
}
