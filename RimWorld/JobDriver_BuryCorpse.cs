using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BuryCorpse : JobDriver
	{
		private const TargetIndex CorpseIndex = TargetIndex.A;

		private const TargetIndex GraveIndex = TargetIndex.B;

		private Corpse Corpse
		{
			get
			{
				return (Corpse)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Building_Grave Grave
		{
			get
			{
				return (Building_Grave)base.CurJob.GetTarget(TargetIndex.B).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
			yield return Toils_Haul.CarryHauledThingToContainer();
			Toil prepare = new Toil();
			prepare.defaultCompleteMode = ToilCompleteMode.Delay;
			prepare.defaultDuration = 250;
			prepare.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.pawn.carryTracker.CarriedThing == null)
					{
						Log.Error(this.<>f__this.pawn + " tried to place hauled corpse in grave but is not hauling anything.");
						return;
					}
					if (this.<>f__this.Grave.TryAcceptThing(this.<>f__this.Corpse, true))
					{
						this.<>f__this.pawn.carryTracker.innerContainer.Remove(this.<>f__this.Corpse);
						this.<>f__this.Grave.Notify_CorpseBuried(this.<>f__this.pawn);
						this.<>f__this.pawn.records.Increment(RecordDefOf.CorpsesBuried);
					}
				}
			};
		}
	}
}
