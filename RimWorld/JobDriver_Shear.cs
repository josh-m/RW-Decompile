using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_Shear : JobDriver_GatherAnimalBodyResources
	{
		protected override float WorkTotal
		{
			get
			{
				return 1700f;
			}
		}

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return animal.TryGetComp<CompShearable>();
		}
	}
}
