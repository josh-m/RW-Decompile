using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Flee : JobDriver
	{
		protected const TargetIndex DestInd = TargetIndex.A;

		protected const TargetIndex DangerInd = TargetIndex.B;

		public override bool TryMakePreToilReservations()
		{
			this.pawn.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.job.GetTarget(TargetIndex.A).Cell);
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				atomicWithPrevious = true,
				defaultCompleteMode = ToilCompleteMode.Instant,
				initAction = delegate
				{
					if (this.$this.pawn.IsColonist)
					{
						MoteMaker.MakeColonistActionOverlay(this.$this.pawn, ThingDefOf.Mote_ColonistFleeing);
					}
				}
			};
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		}
	}
}
