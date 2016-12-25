using System;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_ParentInLaw : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (me.GetSpouse() == null)
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Parent.Worker;
			return !worker.InRelation(me, other) && worker.InRelation(me.GetSpouse(), other);
		}
	}
}
