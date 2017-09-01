using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Execute : JobDriver
	{
		protected Pawn Victim
		{
			get
			{
				return (Pawn)base.CurJob.targetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnAggroMentalState(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Victim, PrisonerInteractionModeDefOf.Execution).FailOn(() => !this.<>f__this.Victim.IsPrisonerOfColony || !this.<>f__this.Victim.guest.PrisonerIsSecure);
			yield return new Toil
			{
				initAction = delegate
				{
					ExecutionUtility.DoExecutionByCut(this.<execute>__0.actor, this.<>f__this.Victim);
					ThoughtUtility.GiveThoughtsForPawnExecuted(this.<>f__this.Victim, PawnExecutionKind.GenericBrutal);
					TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, new object[]
					{
						this.<>f__this.pawn,
						this.<>f__this.Victim
					});
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
