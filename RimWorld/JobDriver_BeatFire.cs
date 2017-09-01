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
				return (Fire)base.CurJob.targetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil beat = new Toil();
			Toil approach = new Toil();
			approach.initAction = delegate
			{
				if (this.<>f__this.Map.reservationManager.CanReserve(this.<>f__this.pawn, this.<>f__this.TargetFire, 1, -1, null, false))
				{
					this.<>f__this.pawn.Reserve(this.<>f__this.TargetFire, 1, -1, null);
				}
				this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetFire, PathEndMode.Touch);
			};
			approach.tickAction = delegate
			{
				if (this.<>f__this.pawn.pather.Moving && this.<>f__this.pawn.pather.nextCell != this.<>f__this.TargetFire.Position)
				{
					this.<>f__this.StartBeatingFireIfAnyAt(this.<>f__this.pawn.pather.nextCell, this.<beat>__0);
				}
				if (this.<>f__this.pawn.Position != this.<>f__this.TargetFire.Position)
				{
					this.<>f__this.StartBeatingFireIfAnyAt(this.<>f__this.pawn.Position, this.<beat>__0);
				}
			};
			approach.FailOnDespawnedOrNull(TargetIndex.A);
			approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			approach.atomicWithPrevious = true;
			yield return approach;
			beat.tickAction = delegate
			{
				if (!this.<>f__this.pawn.CanReachImmediate(this.<>f__this.TargetFire, PathEndMode.Touch))
				{
					this.<>f__this.JumpToToil(this.<approach>__1);
				}
				else
				{
					if (this.<>f__this.pawn.Position != this.<>f__this.TargetFire.Position && this.<>f__this.StartBeatingFireIfAnyAt(this.<>f__this.pawn.Position, this.<beat>__0))
					{
						return;
					}
					this.<>f__this.pawn.natives.TryBeatFire(this.<>f__this.TargetFire);
					if (this.<>f__this.TargetFire.Destroyed)
					{
						this.<>f__this.pawn.records.Increment(RecordDefOf.FiresExtinguished);
						this.<>f__this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
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
					this.pawn.CurJob.targetA = fire;
					this.pawn.pather.StopDead();
					base.JumpToToil(nextToil);
					return true;
				}
			}
			return false;
		}
	}
}
