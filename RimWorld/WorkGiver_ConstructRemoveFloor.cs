using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructRemoveFloor : WorkGiver_ConstructAffectFloor
	{
		protected override DesignationDef DesDef
		{
			get
			{
				return DesignationDefOf.RemoveFloor;
			}
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c)
		{
			return new Job(JobDefOf.RemoveFloor, c);
		}
	}
}
