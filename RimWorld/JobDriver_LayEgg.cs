using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayEgg : JobDriver
	{
		private const int LayEgg = 500;

		private const TargetIndex LaySpotInd = TargetIndex.A;

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 500
			};
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<finalize>__1.actor;
					Thing forbiddenIfOutsideHomeArea = GenSpawn.Spawn(actor.GetComp<CompEggLayer>().ProduceEgg(), actor.Position, this.<>f__this.Map);
					forbiddenIfOutsideHomeArea.SetForbiddenIfOutsideHomeArea();
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
