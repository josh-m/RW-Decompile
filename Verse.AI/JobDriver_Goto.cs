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
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.pawn.mindState != null && this.<>f__this.pawn.mindState.forcedGotoPosition == this.<>f__this.TargetA.Cell)
					{
						this.<>f__this.pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
					}
					if (this.<>f__this.CurJob.exitMapOnArrival && this.<>f__this.pawn.Position.OnEdge())
					{
						this.<>f__this.pawn.ExitMap();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
