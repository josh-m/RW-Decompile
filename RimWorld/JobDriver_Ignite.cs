using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Ignite : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnBurningImmobile(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.pawn.natives.TryStartIgnite(this.<>f__this.TargetThingA);
				}
			};
		}
	}
}
