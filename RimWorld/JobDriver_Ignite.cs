using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Ignite : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnBurningImmobile(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					this.$this.pawn.natives.TryStartIgnite(this.$this.TargetThingA);
				}
			};
		}
	}
}
