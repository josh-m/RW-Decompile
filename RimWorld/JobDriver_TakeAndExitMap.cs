using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TakeAndExitMap : JobDriver
	{
		private const TargetIndex ItemInd = TargetIndex.A;

		private const TargetIndex ExitCellInd = TargetIndex.B;

		protected Thing Item
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			gotoCell.AddPreTickAction(delegate
			{
				if (this.<>f__this.Map.exitMapGrid.IsExitCell(this.<>f__this.pawn.Position))
				{
					this.<>f__this.pawn.ExitMap(true);
				}
			});
			yield return gotoCell;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.pawn.Position.OnEdge(this.<>f__this.pawn.Map) || this.<>f__this.pawn.Map.exitMapGrid.IsExitCell(this.<>f__this.pawn.Position))
					{
						this.<>f__this.pawn.ExitMap(true);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
