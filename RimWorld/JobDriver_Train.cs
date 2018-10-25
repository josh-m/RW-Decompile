using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Train : JobDriver_InteractAnimal
	{
		protected override bool CanInteractNow
		{
			get
			{
				return !TrainableUtility.TrainedTooRecently(base.Animal);
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in base.MakeNewToils())
			{
				yield return toil;
			}
			this.FailOn(() => this.$this.Animal.training.NextTrainableToTrain() == null && !this.$this.OnLastToil);
		}

		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryTrain(TargetIndex.A);
		}
	}
}
