using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Repair : JobDriver
	{
		private const float WarmupTicks = 80f;

		private const float TicksBetweenRepairs = 20f;

		protected float ticksToNextRepair;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil repair = new Toil();
			repair.initAction = delegate
			{
				this.<>f__this.ticksToNextRepair = 80f;
			};
			repair.tickAction = delegate
			{
				Pawn actor = this.<repair>__0.actor;
				actor.skills.Learn(SkillDefOf.Construction, 0.275f, false);
				float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				this.<>f__this.ticksToNextRepair -= statValue;
				if (this.<>f__this.ticksToNextRepair <= 0f)
				{
					this.<>f__this.ticksToNextRepair += 20f;
					this.<>f__this.TargetThingA.HitPoints++;
					this.<>f__this.TargetThingA.HitPoints = Mathf.Min(this.<>f__this.TargetThingA.HitPoints, this.<>f__this.TargetThingA.MaxHitPoints);
					this.<>f__this.Map.listerBuildingsRepairable.Notify_BuildingRepaired((Building)this.<>f__this.TargetThingA);
					if (this.<>f__this.TargetThingA.HitPoints == this.<>f__this.TargetThingA.MaxHitPoints)
					{
						actor.records.Increment(RecordDefOf.ThingsRepaired);
						actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
				}
			};
			repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			repair.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
			repair.defaultCompleteMode = ToilCompleteMode.Never;
			yield return repair;
		}
	}
}
