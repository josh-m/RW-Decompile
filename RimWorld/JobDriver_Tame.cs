using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Tame : JobDriver_InteractAnimal
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in base.MakeNewToils())
			{
				yield return toil;
			}
			this.FailOn(() => this.$this.Map.designationManager.DesignationOn(this.$this.Animal, DesignationDefOf.Tame) == null && !this.$this.OnLastToil);
		}

		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryRecruit(TargetIndex.A);
		}
	}
}
