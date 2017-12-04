using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_Goto : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			this.pawn.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.job.targetA.Cell);
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			gotoCell.AddPreTickAction(delegate
			{
				if (this.$this.job.exitMapOnArrival && this.$this.pawn.Map.exitMapGrid.IsExitCell(this.$this.pawn.Position))
				{
					this.$this.TryExitMap();
				}
			});
			gotoCell.FailOn(() => this.$this.job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.$this.pawn));
			yield return gotoCell;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.$this.pawn.mindState != null && this.$this.pawn.mindState.forcedGotoPosition == this.$this.TargetA.Cell)
					{
						this.$this.pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
					}
					if (this.$this.job.exitMapOnArrival && (this.$this.pawn.Position.OnEdge(this.$this.pawn.Map) || this.$this.pawn.Map.exitMapGrid.IsExitCell(this.$this.pawn.Position)))
					{
						this.$this.TryExitMap();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private void TryExitMap()
		{
			if (this.job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.pawn))
			{
				return;
			}
			this.pawn.ExitMap(true);
		}
	}
}
