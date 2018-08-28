using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_Follow : JobDriver
	{
		private const TargetIndex FolloweeInd = TargetIndex.A;

		private const int Distance = 4;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return new Toil
			{
				tickAction = delegate
				{
					Pawn pawn = (Pawn)this.$this.job.GetTarget(TargetIndex.A).Thing;
					if (this.$this.pawn.Position.InHorDistOf(pawn.Position, 4f) && this.$this.pawn.Position.WithinRegions(pawn.Position, this.$this.Map, 2, TraverseParms.For(this.$this.pawn, Danger.Deadly, TraverseMode.ByPawn, false), RegionType.Set_Passable))
					{
						return;
					}
					if (!this.$this.pawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						this.$this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					if (!this.$this.pawn.pather.Moving || this.$this.pawn.pather.Destination != pawn)
					{
						this.$this.pawn.pather.StartPath(pawn, PathEndMode.Touch);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}

		public override bool IsContinuation(Job j)
		{
			return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}
	}
}
