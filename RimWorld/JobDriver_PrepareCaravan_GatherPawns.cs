using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherPawns : JobDriver
	{
		private const TargetIndex AnimalOrSlaveInd = TargetIndex.A;

		private Pawn AnimalOrSlave
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.<>f__this.Map.lordManager.lords.Contains(this.<>f__this.CurJob.lord));
			this.FailOn(() => this.<>f__this.AnimalOrSlave.GetLord() != this.<>f__this.CurJob.lord);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A).FailOn(() => GatherAnimalsAndSlavesForCaravanUtility.IsFollowingAnyone(this.<>f__this.AnimalOrSlave));
			yield return this.SetFollowerToil();
		}

		private Toil SetFollowerToil()
		{
			return new Toil
			{
				initAction = delegate
				{
					GatherAnimalsAndSlavesForCaravanUtility.SetFollower(this.AnimalOrSlave, this.pawn);
					RestUtility.WakeUp(this.pawn);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
