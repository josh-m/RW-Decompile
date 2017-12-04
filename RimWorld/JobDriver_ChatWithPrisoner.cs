using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ChatWithPrisoner : JobDriver
	{
		protected Pawn Talkee
		{
			get
			{
				return (Pawn)this.job.targetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnMentalState(TargetIndex.A);
			this.FailOnNotAwake(TargetIndex.A);
			this.FailOn(() => !this.$this.Talkee.IsPrisonerOfColony || !this.$this.Talkee.guest.PrisonerIsSecure);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.ConvinceRecruitee(this.pawn, this.Talkee);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.ConvinceRecruitee(this.pawn, this.Talkee);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.ConvinceRecruitee(this.pawn, this.Talkee);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.ConvinceRecruitee(this.pawn, this.Talkee);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.ConvinceRecruitee(this.pawn, this.Talkee);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Talkee, this.Talkee.guest.interactionMode);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
			if (this.job.def == JobDefOf.PrisonerAttemptRecruit)
			{
				yield return Toils_Interpersonal.TryRecruit(TargetIndex.A);
			}
		}
	}
}
