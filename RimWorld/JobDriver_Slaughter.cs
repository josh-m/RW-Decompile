using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Slaughter : JobDriver
	{
		private const int SlaughterDuration = 180;

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
			this.FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Slaughter);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.WaitWith(TargetIndex.A, 180, true, false);
			yield return new Toil
			{
				initAction = delegate
				{
					ExecutionUtility.DoExecutionByCut(this.<execute>__0.actor, this.<>f__this.Victim);
					this.<>f__this.pawn.records.Increment(RecordDefOf.AnimalsSlaughtered);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
