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
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Interpersonal.GotoPrisoner(this.pawn, this.Victim, PrisonerInteractionMode.Execution).FailOn(() => !this.<>f__this.Victim.IsPrisonerOfColony || !this.<>f__this.Victim.guest.PrisonerIsSecure);
			yield return new Toil
			{
				initAction = delegate
				{
					BodyPartDamageInfo value = new BodyPartDamageInfo(this.<>f__this.Victim.health.hediffSet.GetBrain(), false, null);
					this.<>f__this.Victim.TakeDamage(new DamageInfo(DamageDefOf.ExecutionCut, 99999, this.<execute>__0.actor, new BodyPartDamageInfo?(value), null));
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
