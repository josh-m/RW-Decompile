using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Repair : JobDriver
	{
		private const float TicksBetweenRepairs = 12f;

		protected float ticksToNextRepair;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil repair = new Toil();
			repair.tickAction = delegate
			{
				Pawn actor = this.<repair>__0.actor;
				actor.skills.Learn(SkillDefOf.Construction, 0.275f);
				float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				this.<>f__this.ticksToNextRepair -= statValue;
				if (this.<>f__this.ticksToNextRepair <= 0f)
				{
					this.<>f__this.ticksToNextRepair += 12f;
					this.<>f__this.TargetThingA.TakeDamage(new DamageInfo(DamageDefOf.Repair, 1, actor, null, null));
					if (this.<>f__this.TargetThingA.HitPoints == this.<>f__this.TargetThingA.MaxHitPoints)
					{
						actor.records.Increment(RecordDefOf.ThingsRepaired);
						actor.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				}
			};
			repair.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
			repair.defaultCompleteMode = ToilCompleteMode.Never;
			yield return repair;
		}
	}
}
