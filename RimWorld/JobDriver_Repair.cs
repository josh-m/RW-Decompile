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
		protected float ticksToNextRepair;

		private const float WarmupTicks = 80f;

		private const float TicksBetweenRepairs = 20f;

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil repair = new Toil();
			repair.initAction = delegate
			{
				this.$this.ticksToNextRepair = 80f;
			};
			repair.tickAction = delegate
			{
				Pawn actor = repair.actor;
				actor.skills.Learn(SkillDefOf.Construction, 0.275f, false);
				float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				this.$this.ticksToNextRepair -= statValue;
				if (this.$this.ticksToNextRepair <= 0f)
				{
					this.$this.ticksToNextRepair += 20f;
					this.$this.TargetThingA.HitPoints++;
					this.$this.TargetThingA.HitPoints = Mathf.Min(this.$this.TargetThingA.HitPoints, this.$this.TargetThingA.MaxHitPoints);
					this.$this.Map.listerBuildingsRepairable.Notify_BuildingRepaired((Building)this.$this.TargetThingA);
					if (this.$this.TargetThingA.HitPoints == this.$this.TargetThingA.MaxHitPoints)
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
