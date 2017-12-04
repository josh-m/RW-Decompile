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
				return (Corpse)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Building_Grave Grave
		{
			get
			{
				return (Building_Grave)this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public JobDriver_BuryCorpse()
		{
			this.rotateToFace = TargetIndex.B;
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Corpse, this.job, 1, -1, null) && this.pawn.Reserve(this.Grave, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			this.FailOn(() => !this.$this.Grave.Accepts(this.$this.Corpse));
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			yield return Toils_Haul.CarryHauledThingToContainer();
			Toil prepare = Toils_General.Wait(250);
			prepare.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.$this.pawn.carryTracker.CarriedThing == null)
					{
						Log.Error(this.$this.pawn + " tried to place hauled corpse in grave but is not hauling anything.");
						return;
					}
					if (this.$this.Grave.TryAcceptThing(this.$this.Corpse, true))
					{
						this.$this.pawn.carryTracker.innerContainer.Remove(this.$this.Corpse);
						this.$this.Grave.Notify_CorpseBuried(this.$this.pawn);
						this.$this.pawn.records.Increment(RecordDefOf.CorpsesBuried);
					}
				}
			};
		}

		public override object[] TaleParameters()
		{
			return new object[]
			{
				this.pawn,
				(this.Grave.Corpse == null) ? null : this.Grave.Corpse.InnerPawn
			};
		}
	}
}
