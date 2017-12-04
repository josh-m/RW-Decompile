using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SingleInteraction : JobDriver
	{
		private const TargetIndex OtherPawnInd = TargetIndex.A;

		private Pawn OtherPawn
		{
			get
			{
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.OtherPawn, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
			Toil finalGoto = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			finalGoto.socialMode = RandomSocialMode.Off;
			yield return finalGoto;
			yield return Toils_Interpersonal.Interact(TargetIndex.A, this.job.interaction);
		}
	}
}
