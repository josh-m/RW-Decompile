using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Flee : JobDriver
	{
		protected const TargetIndex DestInd = TargetIndex.A;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				atomicWithPrevious = true,
				defaultCompleteMode = ToilCompleteMode.Instant,
				initAction = delegate
				{
					this.<>f__this.Map.pawnDestinationManager.ReserveDestinationFor(this.<>f__this.pawn, this.<>f__this.CurJob.GetTarget(TargetIndex.A).Cell);
					if (this.<>f__this.pawn.IsColonist)
					{
						MoteMaker.MakeColonistActionOverlay(this.<>f__this.pawn, ThingDefOf.Mote_ColonistFleeing);
					}
				}
			};
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		}
	}
}
