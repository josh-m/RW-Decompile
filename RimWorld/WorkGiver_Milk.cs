using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_Milk : WorkGiver_GatherAnimalBodyResources
	{
		protected override JobDef JobDef
		{
			get
			{
				return JobDefOf.Milk;
			}
		}

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompMilkable>();
		}
	}
}
