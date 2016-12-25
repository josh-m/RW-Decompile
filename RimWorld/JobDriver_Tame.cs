using System;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Tame : JobDriver_InteractAnimal
	{
		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryRecruit(TargetIndex.A);
		}
	}
}
