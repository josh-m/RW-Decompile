using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BeatFire : JobDriver
	{
		protected Fire TargetFire
		{
			get
			{
				return (Fire)this.job.targetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil beat = new Toil();
			Toil approach = new Toil();
			approach.initAction = delegate
			{
				if (this.$this.Map.reservationManager.CanReserve(this.$this.pawn, this.$this.TargetFire, 1, -1, null, false))
				{
					this.$this.pawn.Reserve(this.$this.TargetFire, this.$this.job, 1, -1, null, true);
				}
				this.$this.pawn.pather.StartPath(this.$this.TargetFire, PathEndMode.Touch);
			};
			approach.tickAction = delegate
			{
				if (this.$this.pawn.pather.Moving && this.$this.pawn.pather.nextCell != this.$this.TargetFire.Position)
				{
					this.$this.StartBeatingFireIfAnyAt(this.$this.pawn.pather.nextCell, beat);
				}
				if (this.$this.pawn.Position != this.$this.TargetFire.Position)
				{
					this.$this.StartBeatingFireIfAnyAt(this.$this.pawn.Position, beat);
				}
			};
			approach.FailOnDespawnedOrNull(TargetIndex.A);
			approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			approach.atomicWithPrevious = true;
			yield return approach;
			beat.tickAction = delegate
			{
				if (!this.$this.pawn.CanReachImmediate(this.$this.TargetFire, PathEndMode.Touch))
				{
					this.$this.JumpToToil(approach);
				}
				else
				{
					if (this.$this.pawn.Position != this.$this.TargetFire.Position && this.$this.StartBeatingFireIfAnyAt(this.$this.pawn.Position, beat))
					{
						return;
					}
					this.$this.pawn.natives.TryBeatFire(this.$this.TargetFire);
					if (this.$this.TargetFire.Destroyed)
					{
						this.$this.pawn.records.Increment(RecordDefOf.FiresExtinguished);
						this.$this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
						return;
					}
				}
			};
			beat.FailOnDespawnedOrNull(TargetIndex.A);
			beat.defaultCompleteMode = ToilCompleteMode.Never;
			yield return beat;
		}

		private bool StartBeatingFireIfAnyAt(IntVec3 cell, Toil nextToil)
		{
			List<Thing> thingList = cell.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Fire fire = thingList[i] as Fire;
				if (fire != null && fire.parent == null)
				{
					this.job.targetA = fire;
					this.pawn.pather.StopDead();
					base.JumpToToil(nextToil);
					return true;
				}
			}
			return false;
		}
	}
}
