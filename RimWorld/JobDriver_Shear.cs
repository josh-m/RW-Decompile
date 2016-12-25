using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_Shear : JobDriver_GatherAnimalBodyResources
	{
		protected override int Duration
		{
			get
			{
				return 2000;
			}
		}

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompShearable>();
		}
	}
}
