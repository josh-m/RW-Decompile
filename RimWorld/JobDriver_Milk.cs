using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_Milk : JobDriver_GatherAnimalBodyResources
	{
		protected override int Duration
		{
			get
			{
				return 500;
			}
		}

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompMilkable>();
		}
	}
}
