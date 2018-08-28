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

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return Toils_General.Wait(500, TargetIndex.None);
			yield return Toils_General.Do(delegate
			{
				Thing forbiddenIfOutsideHomeArea = GenSpawn.Spawn(this.$this.pawn.GetComp<CompEggLayer>().ProduceEgg(), this.$this.pawn.Position, this.$this.Map, WipeMode.Vanish);
				forbiddenIfOutsideHomeArea.SetForbiddenIfOutsideHomeArea();
			});
		}
	}
}
