using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_OperateDeepDrill : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(delegate
			{
				CompDeepDrill compDeepDrill = this.$this.job.targetA.Thing.TryGetComp<CompDeepDrill>();
				return !compDeepDrill.CanDrillNow();
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil work = new Toil();
			work.tickAction = delegate
			{
				Pawn actor = work.actor;
				Building building = (Building)actor.CurJob.targetA.Thing;
				CompDeepDrill comp = building.GetComp<CompDeepDrill>();
				comp.DrillWorkDone(actor);
				actor.skills.Learn(SkillDefOf.Mining, 0.0714999959f, false);
			};
			work.defaultCompleteMode = ToilCompleteMode.Never;
			work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);
			work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return work;
		}
	}
}
