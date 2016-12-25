using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Kin : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			return me != other && me.relations.FamilyByBlood.Contains(other);
		}
	}
}
