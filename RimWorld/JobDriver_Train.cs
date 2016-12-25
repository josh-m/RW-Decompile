using System;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Train : JobDriver_InteractAnimal
	{
		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryTrain(TargetIndex.A);
		}
	}
}
