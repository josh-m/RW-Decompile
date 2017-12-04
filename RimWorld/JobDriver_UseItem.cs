using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UseItem : JobDriver
	{
		private int useDuration = -1;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			this.useDuration = this.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>().Props.useDuration;
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = Toils_General.Wait(this.useDuration);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return prepare;
			Toil use = new Toil();
			use.initAction = delegate
			{
				Pawn actor = use.actor;
				CompUsable compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable>();
				compUsable.UsedBy(actor);
			};
			use.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return use;
		}
	}
}
