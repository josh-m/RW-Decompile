using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse.AI
{
	public class JobDriver_Goto : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			gotoCell.AddPreTickAction(delegate
			{
				if (this.<>f__this.CurJob.exitMapOnArrival && this.<>f__this.pawn.Map.exitMapGrid.IsExitCell(this.<>f__this.pawn.Position))
				{
					this.<>f__this.TryExitMap();
				}
			});
			gotoCell.FailOn(() => this.<>f__this.CurJob.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.<>f__this.pawn));
			yield return gotoCell;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.pawn.mindState != null && this.<>f__this.pawn.mindState.forcedGotoPosition == this.<>f__this.TargetA.Cell)
					{
						this.<>f__this.pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
					}
					if (this.<>f__this.CurJob.exitMapOnArrival && (this.<>f__this.pawn.Position.OnEdge(this.<>f__this.pawn.Map) || this.<>f__this.pawn.Map.exitMapGrid.IsExitCell(this.<>f__this.pawn.Position)))
					{
						this.<>f__this.TryExitMap();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private void TryExitMap()
		{
			if (base.CurJob.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.pawn))
			{
				return;
			}
			this.pawn.ExitMap(true);
		}
	}
}
