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
			Toil approach = new Toil();
			approach.initAction = delegate
			{
				if (this.<>f__this.Map.reservationManager.CanReserve(this.<>f__this.pawn, this.<>f__this.TargetFire, 1))
				{
					this.<>f__this.pawn.Reserve(this.<>f__this.TargetFire, 1);
				}
				this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetFire, PathEndMode.Touch);
			};
			approach.tickAction = delegate
			{
				if (this.<>f__this.pawn.pather.Moving && this.<>f__this.pawn.pather.nextCell != this.<>f__this.TargetFire.Position)
				{
					List<Thing> thingList = this.<>f__this.pawn.pather.nextCell.GetThingList(this.<>f__this.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Fire fire = thingList[i] as Fire;
						if (fire != null && fire.parent == null)
						{
							this.<>f__this.pawn.CurJob.targetA = fire;
							this.<>f__this.pawn.pather.StopDead();
							this.<>f__this.ReadyForNextToil();
							return;
						}
					}
				}
			};
			approach.FailOnDespawnedOrNull(TargetIndex.A);
			approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			approach.atomicWithPrevious = true;
			yield return approach;
			Toil beat = new Toil();
			beat.tickAction = delegate
			{
				if (!this.<>f__this.pawn.Position.AdjacentTo8WayOrInside(this.<>f__this.TargetFire))
				{
					this.<>f__this.pawn.pather.StartPath(this.<>f__this.TargetFire, PathEndMode.Touch);
				}
				else
				{
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
	}
}
